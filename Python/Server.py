import zmq
import zmq.asyncio
import asyncio
#import State_pb2
import kinetics_pb2
import graph_pb2
import numpy as np

'''This file is a dummy server used for testing Unity's ability to receive and deal with messages without involving the vision or C&C code.'''

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
        for i in range(6):
            a = self.graph.nodes[i]
            a.label = 'cstr' #if(i%2==0) else 'pfr'
            a.id = i+1
            a.position.append((i+1) * 0.15)
            a.position.append((0.25 if(i%2 != 1) else (0.75 - (i%3)*0.15)))

            b = self.kinetic_system.kinetics.add()
            b.id = i+1
            b.temperature = 100.0
            b.pressure = 100.0
            b.mole_fraction.append(np.random.uniform() * 0.5)
            for j in range(1,3):
                b.mole_fraction.append(np.random.uniform() * (1.0 - np.sum(b.mole_fraction[:j])))
            b.mole_fraction.append(1.0 - np.sum(b.mole_fraction[:4]))

        a = self.graph.nodes[6]
        a.label = "conditions"
        a.id = 7
        a.position.append(0)
        a.position.append(0)
        a.weight.append(380)
        a.weight.append(2)


        for i in range(2):
            e = self.graph.edges[i]
            e.idA = i
            e.labelA = "cstr"
            e.idB = i+1
            e.labelB = "cstr"
            e.weight.append((0))

        e = self.graph.edges[2]
        e.idA = 1
        e.labelA = "cstr"
        e.idB = 3
        e.labelB = "cstr"
        e.weight.append((0))

        e = self.graph.edges[3]
        e.idA = 2
        e.labelA = "cstr"
        e.idB = 4
        e.labelB = "cstr"
        e.weight.append((0))

        e = self.graph.edges[4]
        e.idA = 3
        e.labelA = "cstr"
        e.idB = 5
        e.labelB = "cstr"
        e.weight.append((0))

        e = self.graph.edges[5]
        e.idA = 4
        e.labelA = "cstr"
        e.idB = 5
        e.labelB = "cstr"
        e.weight.append((0))

        e = self.graph.edges[6]
        e.idA = 5
        e.labelA = "cstr"
        e.idB = 6
        e.labelB = "cstr"
        e.weight.append((0))





    async def update_gamestate(self):
        self.graph.time += 1
        self.kinetic_system.time+=1
        #this part is for checking movement
        #for i in range(6):
            #a = self.graph.nodes[i]
            #a.position[1] = (0.75 - 0.5 * ((self.graph.time)%2)) if(i%2 != 1) else 0.75
        #    holdout = self.kinetic_system.kinetics[i].mole_fraction[0]
        #    for j in range(len(self.kinetic_system.kinetics[i].mole_fraction)):
        #        if(j is not len(self.kinetic_system.kinetics[i].mole_fraction)-1):
        #            self.kinetic_system.kinetics[i].mole_fraction[j] = self.kinetic_system.kinetics[i].mole_fraction[(j+1)]
        #        else:
        #            self.kinetic_system.kinetics[i].mole_fraction[j] = holdout
        #check handling deleted node after some time...
        if(self.graph.time == 10):
            pass
            self.graph.nodes[5].delete = True
            #del self.graph.edges[4]
            #del self.graph.edges[5]
        #print(self.kinetic_system)
        for key in self.graph.nodes:
            a = self.graph.nodes[key]
            #print("key is {}".format(key))
            #if(key > 0):
            #    b = self.kinetic_system.kinetics[key-1]
            if(not a.delete):
                a.position[1] += 0.01*(1 if self.graph.time%2==0 else -1)
                if(len(a.weight) > 1):
                    a.weight[0] += 1
                    #b.temperature = a.weight[0]
                    a.weight[1] += 0.05
                    #b.pressure = a.weight[1]
        if(self.graph.time == 20):
            a = self.graph.nodes[8]
            a.label = "cstr"
            a.id = 9
            a.position.append(0.5)
            a.position.append(0.5)
            a.weight.append(380)
            a.weight.append(2)

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

