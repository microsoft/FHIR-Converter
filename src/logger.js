'use strict'

const pino = require('pino')

// const config = require('./config').getConfig()
const config = require('./config');

console.log(config.get("logLevel"));
const logger = pino({
  level: config.get("logLevel"),
  prettyPrint: {
    colorize: true,
    translateTime: 'sys:UTC:yyyy-mm-dd"T"HH:MM:ss:l"Z"',
    ignore: 'pid,hostname'
  },
  serializers: {
    err: pino.stdSerializers.err
  },
  enabled: true
})

module.exports = logger