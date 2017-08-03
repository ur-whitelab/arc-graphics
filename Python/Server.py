import zmq, zmq.asyncio, asyncio
import GameState_pb2


ctx = zmq.asyncio.Context()

def bind(uri):
    sock = ctx.socket(zmq.PUB)
    sock.bind(uri)
    return sock


async def loop(sock):
    msg = await sock.recv_multipart() # waits for msg to be ready
    

def main():
    loop = asyncio.get_event_loop()
    # Blocking call which returns when the display_date() coroutine is done
    loop.run_until_complete(display_date(loop))
    loop.close()


if __name__ == '__main__':
    main()

