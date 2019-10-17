FROM node:10.16.3-alpine

RUN npm install --global @testmace/cli
COPY ./Project /project

WORKDIR /out
CMD testmace-cli /project --reporter=junit -e testmace
