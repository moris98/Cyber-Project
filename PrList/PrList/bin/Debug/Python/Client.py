from win32com.client import GetObject
import socket
from threading import Thread
from PIL import ImageGrab
import threading
import time
import binascii
import subprocess
import os

import ConnectionModule
UDPip="0.0.0.0"
TCPip="127.0.0.1"
TCPport=8064
UDPport=7014
UDPrespondport=9000

UDPsock=ConnectionModule.SocketConnection("Server","UDP",(UDPip,UDPport))
TCPsock=ConnectionModule.SocketConnection("Client","TCP",(TCPip,TCPport))
TCPsockRespond=ConnectionModule.SocketConnection("Client","TCP",(TCPip,UDPrespondport))

def ProcessList():

    WMI = GetObject('winmgmts:')
    processes = WMI.InstancesOf('Win32_Process')
    len(processes)
    return [process.Properties_('Name').Value for process in processes]
    # proclist=[]
    # procs = psutil.get_process_list()
    # for proc in procs:
    #     cpu_percent = proc.get_cpu_percent()
    #     mem_percent = proc.get_memory_percent()
    #     name = proc.name
    #     proclist.append(name)
    #     proclist.append(cpu_percent)
    #     proclist.append(mem_percent)
    # print proclist
    # return proclist
def Listening_Thread():
    global UDPsock

    while True:
        Request,Serverip=UDPsock.ToRecv(1024)
        print Request
        if  "DEL" in Request or  "START" in Request:
            PROCNAME = Request.split(" ")[1]
            if "DEL" in Request:

                test=os.system("taskkill /im "+PROCNAME)
                if test==128:
                    print "no such process is working"
            if "START" in Request:
                try:
                    subprocess.Popen(PROCNAME)
                except:
                    print "no such process exist"
        if "Screen_Capture" in Request:
            ImageGrab.grab().save("S_C.jpg","JPEG")
            Resporn_send = Thread(target=Send_Screen_Capture)
            Resporn_send.start()


def Send_Screen_Capture():
    global TCPsockRespond
    S_C=open("S_C.jpg","rb")
    Screen_Capture= S_C.read()
    print Screen_Capture
    tosend="IMAGE#"+str(len(Screen_Capture))
    print tosend[6:]
    TCPsockRespond.ToSend("IMAGE#"+str(len(Screen_Capture)))
    TCPsockRespond.ToSend(Screen_Capture)



def sending_ProcessList(sock):
    Mname=socket.gethostname()
    while True:
        strlistproc=Mname
        for i in ProcessList():
            strlistproc=strlistproc+','+str(i)

        TCPsock.ToSend(strlistproc)

        time.sleep(10)
        
def __main__():

    global TCPsock
    listening_thread = Thread(target = Listening_Thread)
    listening_thread.start()
    sending_ProcessList(TCPsock)
    listening_thread.join()
    
    
__main__()
