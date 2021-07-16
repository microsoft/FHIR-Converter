'use strict'

const {parseStringToBoolean} = require('./util')
const nconf = require('nconf');
const env = process.env.NODE_ENV || 'development';

nconf.argv()
  .env({separator:'__'})
  .file(`${__dirname}/../config.json`)

module.exports = nconf;
// exports.getConfig = function () {
//   return Object.freeze({
//     port: process.env.SERVER_PORT || 3003,
//     logLevel: process.env.LOG_LEVEL || 'info',
//     enableLogging: parseStringToBoolean(process.env.ENABLE_LOGGING, true),
//     openhim: Object.freeze({
//       apiURL: process.env.OPENHIM_URL || 'https://localhost:8080',
//       username: process.env.OPENHIM_USERNAME || 'root@openhim.org',
//       password: process.env.OPENHIM_PASSWORD || 'openhim-password',
//       trustSelfSigned: parseStringToBoolean(
//         process.env.TRUST_SELF_SIGNED,
//         true
//       ),
//       register: parseStringToBoolean(process.env.OPENHIM_REGISTER, true),
//       urn: process.env.MEDIATOR_URN || 'urn:mediator:generic_mapper'
//     }),
//     dynamicEndpoints: parseStringToBoolean(process.env.DYNAMIC_ENDPOINTS, true)
//   })
// }

// const nconf = require('nconf');
// const env = process.env.NODE_ENV || 'development';
// let decisionRulesFile;
// if(env === 'test') {
//   decisionRulesFile = `${__dirname}/../config/decisionRulesTest.json`;
// } else {
//   decisionRulesFile = `${__dirname}/../config/decisionRules.json`;
// }
// nconf.argv()
//   .env({separator:'__'})
//   .file(`${__dirname}/../config/config_${env}.json`)
//   .file('decRules', decisionRulesFile);
// module.exports = nconf;