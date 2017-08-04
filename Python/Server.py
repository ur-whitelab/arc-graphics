import zmq, zmq.asyncio, asyncio
import GameState_pb2

class StateServer:


    def __init__(self, uri):
        self.ctx = zmq.asyncio.Context()
        self.sock = self.ctx.socket(zmq.PUB)
        self.sock.bind(uri)
        self.state = GameState_pb2.GameState()
        self.state.time = 0
        #create some random attractors
        for i in range(3):
            a = self.state.attractors.add()
            a.position.append(i * 10.0)
            a.position.append(0.0)
            a.state = GameState_pb2.Attractor.ACTIVE

    async def update_gamestate(self):    
        self.state.time += 1        
        for a in self.state.attractors:
            a.position[1] + 1
        await asyncio.sleep(0.5)
        return self.state
        
    async def loop(self):
        while True:
            state = await self.update_gamestate()
            self.sock.send(state.SerializeToString())
        
    
    
    

def main():
    zmq.asyncio.install()
    server = StateServer('tcp://*:5000')
    loop = asyncio.get_event_loop()
    loop.run_until_complete(server.loop())
    loop.close()
#    context = zmq.Context()
#    sock = context.socket(zmq.PAIR)
#    sock.bind('tcp://*:5000')
#    while True:
#        msg = sock.send(b'hello')
        



if __name__ == '__main__':
    main()

