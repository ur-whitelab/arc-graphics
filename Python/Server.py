import zmq
import zmq.asyncio
import asyncio
import State_pb2
import kinetics_pb2
import graph_pb2


class StateServer:

    connected = False

    def __init__(self, uri):
        self.ctx = zmq.asyncio.Context()
        print('Opening PUB Socket on {}'.format(uri))
        self.sock = self.ctx.socket(zmq.PUB)
        self.sock.connect(uri)
        #for use with state buffer:
        #self.state = State_pb2.StructuresState()
        #self.state.time = 0

        #for use with reactor system buffer:
        self.graph = graph_pb2.Graph()
        self.graph.time = 0

        self.kinetic_system = kinetics_pb2.SystemKinetics()
        self.kinetic_system.time = 0

        #create some reactors
        for i in range(3):
            a = self.graph.nodes[i]
            a.label = str(1)
            a.id = i
            a.position.append((i-1) * 10.0)
            a.position.append(0.0)

            b = self.kinetic_system.kinetics.add()
            b.temperature = 100.0
            b.pressure = 100.0
            b.mole_fraction.append(0.2)
            b.mole_fraction.append(0.2)
            b.mole_fraction.append(0.2 + 0.1*i)
            b.mole_fraction.append(0.4 - i*0.1)


    async def update_gamestate(self):
        self.graph.time += 1
        self.kinetic_system.time+=1
        for i in range(3):
            holdout = self.kinetic_system.kinetics[i].mole_fraction[0]
            for j in range(len(self.kinetic_system.kinetics[i].mole_fraction)):
                if(j is not len(self.kinetic_system.kinetics[i].mole_fraction)-1):
                    self.kinetic_system.kinetics[i].mole_fraction[j] = self.kinetic_system.kinetics[i].mole_fraction[(j+1)]
                else:
                    self.kinetic_system.kinetics[i].mole_fraction[j] = holdout
        print(self.kinetic_system)
        for key in self.graph.nodes:
            a = self.graph.nodes[key]
            #a.position[1] += 3*(a.id+1)*(1 if self.graph.time%2 else -1)
        await asyncio.sleep(1.0)
        return (self.graph, self.kinetic_system)


    async def pubLoop(self):
        counter = 0
        while True:
            graph, kinetics = await self.update_gamestate()
            await self.sock.send_multipart(['vision-update'.encode(), graph.SerializeToString()])
            await self.sock.send_multipart(['simulation-update'.encode(), kinetics.SerializeToString()])
            if(not self.connected):
                self.connected = True
                print('Socket has sent first packet')
            counter += 1
            if counter % 25 == 0:
                print('Packet {} sent'.format(counter))


def main(host='*', port='8075'):
    zmq.asyncio.install()
    server = StateServer('tcp://127.0.0.1:{}'.format(port))
    print('Starting server at {} on port {}'.format(host, port))
    loop = asyncio.get_event_loop()
    loop.run_until_complete(server.pubLoop())
    loop.close()




if __name__ == '__main__':
    main()

