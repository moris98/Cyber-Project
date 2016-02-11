##Connection Module##

import socket,struct

DONE = "#DONE#"

class SocketConnection:
    def __init__(self,position,Connection_Type,addr):
        self.Connection_Type=Connection_Type
        self.position=position
        self.addr=addr
        self.sock= None
        if position=="Client":

            if Connection_Type=="UDP":
                self.sock= socket.socket(socket.AF_INET,socket.SOCK_DGRAM)

            elif Connection_Type=="TCP":
                self.sock= socket.socket()
                self.sock.connect(addr)

            else :
                return
        elif position=="Server":
            if Connection_Type=="UDP":
                self.sock= socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
                self.sock.bind(addr)

            elif Connection_Type=="TCP":
                self.sock = socket.socket()
                self.sock.bind(addr)
                self.sock.listen(15)
            else :
                return

    def ToSend(self,info):
        if self.Connection_Type=="UDP":

            print info

            self.sock.sendto(info[0],info[1])


        elif self.Connection_Type=="TCP":
            self.sock.send(info)
        else:
            return

    def ToRecv(self,n):
        if self.Connection_Type=="UDP":
            return self.sock.recvfrom(n)
        if self.Connection_Type=="TCP":
            return self.sock.recv(n)

    def Get_Addr(self):
        return self.addr

    def ToAccept(self):
        self.sock,x=self.sock.accept()
        return (self.sock,x)


class PipeConnection:
    def __init__(self, in_f , out_f, Respond_f):
        import threading
        self.Respond_f=Respond_f
        self.out_f = out_f
        self.in_f = in_f
        self.lock=threading.Semaphore(1)

    def ToSend(self, item):
        z = [item[x:min(len(item), x+1024)] for x in xrange(0, len(item),1024)]
        print z
        self.lock.acquire(1)
        for i in z:

            try:
                self.out_f.write(struct.pack('I', len(i)) + i)
            except:
                break

            n = struct.unpack('I', self.Respond_f.read(4))[0]
            if self.Respond_f.read(n)=="ok":
               print "ok"
               continue
        print "before sending done"

        self.out_f.write(struct.pack('I', len(DONE)) + DONE)
        self.out_f.flush()
        self.lock.release()

    def ToRecv(self,n):      
         data =  self.in_f.read(n)
         print data
         return data





