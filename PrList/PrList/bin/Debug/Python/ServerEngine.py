import socket
from threading import Thread
import threading
import sys
import time
import struct
import os
import ConnectionModule
import time

TCPip="127.0.0.1"
TCPport=7014



server_client = open(r'\\.\pipe\Orders', 'r+b',0)
client_server= open(r'\\.\pipe\Data', 'r+b', 0)
respond= open(r'\\.\pipe\Respond', 'r+b', 0)
UDPsock=ConnectionModule.SocketConnection("Client","UDP",(TCPip,TCPport))
TCPsockRespond=ConnectionModule.SocketConnection("Server","TCP",("0.0.0.0",9000))
PIPE=ConnectionModule.PipeConnection(server_client,client_server,respond)

options={}
lock=threading.Semaphore(1)
Clients_dic={}

def handler(TCPsock,addr):
    global Clients_dic
    global client_server
    while True:
        data=TCPsock.ToRecv(4096)
        Clients_dic[data.split(",")[0]]=addr
        PIPE.ToSend(data)

def Pipe_server_client():
    while True:

        n = struct.unpack('I', PIPE.ToRecv(4))[0]    # Read str length
        Data = PIPE.ToRecv(n)                           # Read str
        server_client.seek(0)
        #Data=server_client.readline()
        Client_name=Data.split(",")[0]
        addr=Clients_dic[Client_name]
        order=(Data.split(",")[1], (addr[0],7014))
        UDPsock.ToSend(order)
        if not "DEL" in order and not "START" in order:
            Resporn_Wait = Thread(target=Image_Capture)
            Resporn_Wait.start()

def Image_Capture():
            global TCPsockRespond,PIPE
            (clientsock_Reciving, addr) = TCPsockRespond.ToAccept()
            IMAGE_MSG=TCPsockRespond.ToRecv(1024)
            PIPE.ToSend(IMAGE_MSG)
            print "here: "+str(IMAGE_MSG)
            Respond=""
            print int(IMAGE_MSG[6:])/2048+1
            for x in range(0,int(IMAGE_MSG[6:])/2048+1):
                image=TCPsockRespond.ToRecv(2048)
                Respond+=image
            print str(len(Respond))+"GOT IT"
            PIPE.ToSend(Respond)

def connecting():


    while 1:
        TCPsock=ConnectionModule.SocketConnection("Server","TCP",("0.0.0.0",8064))
        print 'waiting for connection...'
        (clientsock_Reciving, addr) = TCPsock.ToAccept()

        print '...connected from:', addr,' For Sending Info'
        t1 = Thread(target=handler, args=(TCPsock,addr,))
        t1.start()

if __name__=='__main__':

    Pipe_server_client_Thread = Thread(target=Pipe_server_client)
    Pipe_server_client_Thread.start()
    t =Thread(target=connecting) 
    t.start()
