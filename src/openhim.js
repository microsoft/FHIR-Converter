'use strict'

const fs = require('fs')
const path = require('path')

const logger = require('./logger')
const config = require('./config')

const mediatorUtils = require('openhim-mediator-utils')

let mediatorConfigJson, readError

// try {
//   // const mediatorConfigFile = fs.readFileSync(
//   //   path.resolve(__dirname, '..', 'mediatorConfig.json')
//   // )
//   // mediatorConfigJson = JSON.parse(mediatorConfigFile)
// } catch (err) {
//   readError = err.message
//   logger.error(`Mediator config file could not be retrieved: ${err.message}`)
// }

const openhimConfig = Object.assign(
  {urn: config.get('mediatorConfig:urn')},
  config.get('mediator:api')
)

mediatorConfigJson = config.get('mediatorConfig')

const mediatorSetup = () => {
  mediatorUtils.registerMediator(openhimConfig, mediatorConfigJson, error => {
    if (error) {
      logger.error(`Failed to register mediator. Caused by: ${error.message}`)
      throw error
    }

    logger.info('Successfully registered mediator!')

    const emitter = mediatorUtils.activateHeartbeat(openhimConfig)

    emitter.on('error', openhimError => {
      logger.error('Heartbeat failed: ', openhimError)
    })
  })
}

exports.constructOpenhimResponse = (ctx, responseTimestamp) => {
  const response = ctx.response
  const orchestrations = ctx.orchestrations
  const statusText = ctx.statusText
  const respObject = {}

  // content-type already defined by final primary request
  if (response.type === 'application/json+openhim') {
    if (orchestrations) {
      if (response.body) {
        if (
          response.body.orchestrations &&
          Array.isArray(response.body.orchestrations)
        ) {
          response.body.orchestrations = orchestrations.concat(
            response.body.orchestrations
          )
        } else {
          response.body.orchestrations = orchestrations
        }
      }
    }

    ctx.body =
      typeof response.body === 'string'
        ? response.body
        : JSON.stringify(response.body)
    return
  }

  // OpenHIM header not explicity set in response header
  // Manually set OpenHIM header for processing
  ctx.response.type = 'application/json+openhim'

  if (response) {
    if (response.headers) {
      respObject.headers = response.headers
    }
    if (response.status) {
      respObject.status = response.status
    }
    if (response.body) {
      respObject.body =
        typeof response.body === 'string'
          ? response.body
          : JSON.stringify(response.body)
    }
    if (response.timestamp) {
      respObject.timestamp = response.timestamp
    } else if (responseTimestamp) {
      respObject.timestamp = responseTimestamp
    }
  }

  if (readError) {
    mediatorConfigJson = {
      urn: 'undefined'
    }
  }

  const body = {
    'x-mediator-urn': mediatorConfigJson.urn,
    status: statusText,
    response: respObject,
    orchestrations: orchestrations
  }

  ctx.body = JSON.stringify(body)
}

exports.mediatorSetup = mediatorSetup