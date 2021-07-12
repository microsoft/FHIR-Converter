#########################################
### Base Image                         ##
#########################################
FROM node:14 AS build
ARG version=v2.1.1
RUN git clone --depth=1 --branch ${version} https://github.com/microsoft/FHIR-Converter.git /app
WORKDIR /app
COPY . /app
RUN npm install --only=production --no-fund --no-optional --no-audit

#########################################
### Prod Image                         ##
#########################################
FROM node:14-slim
RUN  apt-get update && apt install libcurl4-gnutls-dev -y && apt autoremove -y
COPY --from=build /app /app
WORKDIR /app
RUN ["chmod", "+x", "/app/deploy/webapp.sh"]
EXPOSE 2019
CMD [ "npm", "start" ]
