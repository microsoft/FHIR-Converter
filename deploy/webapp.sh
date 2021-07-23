#!/bin/bash

# symlink for web app persistence
mkdir -p /home/fhirconvertertemplates
ln -snf /home/fhirconvertertemplates /usr/dist/app/src/service-templates

npm start
