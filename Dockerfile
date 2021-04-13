#########################################
### Base Image                         ##
#########################################
FROM node:10 AS build
ARG version=v2.1.0
copy . /app
WORKDIR /app
RUN npm install --only=production --no-fund --no-optional --no-audit

#########################################
### Prod Image                         ##
#########################################
FROM node:10-slim
RUN useradd -ms /bin/bash appuser && mkdir /tmp/template_repo && chown appuser:appuser /tmp/template_repo
RUN  apt-get update && apt install libcurl4-gnutls-dev -y && apt autoremove -y
COPY --from=build --chown=appuser /app /app
WORKDIR /app

USER appuser
EXPOSE 2019
CMD [ "npm", "start" ]
