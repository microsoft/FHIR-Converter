FROM node:10

# Create app directory
WORKDIR /usr/src/app

# Install app dependencies
# Copy package.json first for package install
COPY package*.json ./

# Lay down packages as separate layer
RUN apt remove imagemagick* -y && apt remove libmagick* -y && apt autoremove -y && apt-get update && apt-get upgrade -y &&  npm install

# Bundle app source
COPY . .

RUN ["chmod", "+x", "/usr/src/app/deploy/webapp.sh"]

EXPOSE 2019
CMD [ "npm", "start" ]