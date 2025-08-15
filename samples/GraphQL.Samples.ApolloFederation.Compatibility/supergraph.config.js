// PM2 config
const path = require('path');

const federationTestsPath = process.env.FEDERATION_TESTS_PATH || './federation-tests';

module.exports = {
    apps : [{
      name   : "subgraph inventory",
      script : "index.js",
      cwd: path.join(__dirname, federationTestsPath, 'dist/subgraphs/inventory'),
      wait_ready: true
    },{
      name   : "subgraph users",
      script : "index.js",
      cwd: path.join(__dirname, federationTestsPath, 'dist/subgraphs/users'),
      wait_ready: true
    }]
  }
