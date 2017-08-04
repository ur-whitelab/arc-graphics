import zmq

context = zmq.Context()
sock = context.socket(zmq.PAIR)
#sock.setsockopt(zmq.SUBSCRIBE, b'')
sock.connect('tcp://172.17.0.2:5000')
while True:
    msg = sock.recv()
    print(msg)
