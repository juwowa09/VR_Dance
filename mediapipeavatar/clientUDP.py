import socket
import time
import threading

class ClientUDP(threading.Thread):

    def run(self):
        self.connect()

    def __init__(self,ip,port, autoReconnect = True) -> None:
        threading.Thread.__init__(self)
        self.ip = ip
        self.port = port
        self.autoReconnect = autoReconnect # 연결이 끊기면 계속 자동으로 연결하도록 설정
        self.connected = False
        pass

    def isConnected(self):
        return self.connected

    def sendMessage(self,message):
        try:
            message = str('%s<EOM>'%message).encode('utf-8')
            self.socket.send(message)
        except ConnectionRefusedError as ex:
            print("Connection refused. Is server running?")
            self.disconnect()
        except ConnectionResetError as ex:
            print("Server was disconnected...")
            self.disconnect()

    def disconnect(self):
        self.connected = False
        self.socket.close()
        if(self.autoReconnect): # 1초후 다시 연결
            time.sleep(1)
            self.connect()

    def connect(self): # 소켓 통신 사용
        try:
            self.socket = socket.socket(socket.AF_INET, 
                                        socket.SOCK_DGRAM)     
            print("Attempting Connection...")
            self.socket.connect((self.ip, self.port))
            print("Will send messages to "+str(self.socket.getpeername()))
            self.connected = True
        except ConnectionRefusedError as ex:
            print("Connection refused. Is server running?")
            self.disconnect()
        except ConnectionResetError as ex:
            print("Server was disconnected...")
            self.disconnect()
        