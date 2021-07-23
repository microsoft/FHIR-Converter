#########################################
### Base Image                         ##
#########################################
FROM node:14 AS build
COPY ./src /app/src
COPY ./package.json /app 
COPY ./.eslintrc.js /app
COPY ./tsconfig.json /app

WORKDIR /app

RUN npm install --no-fund --no-optional --no-audit
RUN npm run-script build

#########################################
### Prod Image                         ##
#########################################
FROM node:14-slim
RUN  apt-get update && apt install libcurl3 libcurl4-gnutls-dev -y && apt autoremove -y

COPY --from=build /app /app
COPY ./deploy /app/deploy
COPY ./data /app/data
COPY ./static /app/static

WORKDIR /app
RUN ["chmod", "+x", "/app/deploy/webapp.sh"]
EXPOSE 2019
ENTRYPOINT [ "./deploy/webapp.sh" ]
