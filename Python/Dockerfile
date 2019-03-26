FROM ubuntu

RUN apt-get update && apt-get install -y python3-zmq python3-pip
RUN pip3 install protobuf

ENV SHELL /bin/bash
ENV USER physics
ENV UID 1000
ENV HOME /home/$USER
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US.UTF-8

# Create user with UID=1000 and in the 'users' group
RUN useradd -m -s /bin/bash -N -u $UID $USER
USER $USER
WORKDIR /home/$USER