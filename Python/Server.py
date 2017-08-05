dimport zmq, zmq.asyncio, asyncio
import State_pb2

class StateServer:
    def __init__(self, uri):
        self.ctx = zmq.asyncio.Context()
        self.sock = self.ctx.socket(zmq.PUB)
        self.sock.bind(uri)
        self.state = State_pb2.StructuresState()
        self.state.time = 0
        #create some random attractors
        for i in range(3):
            a = self.state.structures.add()
            a.type = 0
            a.id = i
            a.position.append(i * 10.0)
            a.position.append(0.0)

    async def update_gamestate(self):    
        self.state.time += 1        
        for a in self.state.structures:
            a.position[1] += 0.01
        await asyncio.sleep(0.02)
        return self.state
        
    async def pubLoop(self):
        while True:
            state = await self.update_gamestate()
            self.sock.send(state.SerializeToString())           

def main():
    zmq.asyncio.install()
    server = StateServer('tcp://*:5000')
    loop = asyncio.get_event_loop()
    loop.run_until_complete(server.pubLoop())
    loop.close()
        



if __name__ == '__main__':
    main()

