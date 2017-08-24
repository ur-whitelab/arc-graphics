import zmq
import zmq.asyncio
import asyncio
import State_pb2
import kinetics_pb2
import reactors_pb2


class StateServer:

    connected = False

    def __init__(self, uri):
        self.ctx = zmq.asyncio.Context()
        print('Opening PUB Socket on {}'.format(uri))
        self.sock = self.ctx.socket(zmq.PUB)
        self.sock.bind(uri)
        self.state = State_pb2.StructuresState()
        self.state.time = 0

        #create some random attractors
        for i in range(3):
            a = self.state.structures.add()
            a.type = 1
            a.id = i
            a.position.append(i * 10.0)
            a.position.append(0.0)
            b = self.state.structures.add()
            b.type = 0
            b.id = i
            b.position.append(i * 10.0)
            b.position.append(0.0)
            

    async def update_gamestate(self):
        self.state.time += 1
        for a in self.state.structures:
            a.position[1] += 3*(1 if self.state.time%2 else -1)
        await asyncio.sleep(1)
        return self.state

    async def pubLoop(self):
        counter = 0
        while True:
            state = await self.update_gamestate()
            self.sock.send(state.SerializeToString())
            if(not self.connected):
                self.connected = True
                print('Socket has sent first packet')
            counter += 1
            if counter % 100 == 0:
                print('Packet {} sent'.format(counter))


def main(host='*', port='5000'):
    zmq.asyncio.install()
    server = StateServer('tcp://*:5000')
    print('Starting server at {} on port {}'.format(host, port))
    loop = asyncio.get_event_loop()
    loop.run_until_complete(server.pubLoop())
    loop.close()




if __name__ == '__main__':
    main()

