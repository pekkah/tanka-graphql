// PM2 config
module.exports = {
    apps : [{
      name   : "subgraph inventory",
      script : "index.js",
      cwd: "/home/pekkah/.nvm/versions/node/v24.2.0/lib/node_modules/@apollo/federation-subgraph-compatibility/node_modules/@apollo/federation-subgraph-compatibility-tests/dist/subgraphs/inventory",
      wait_ready: true
    },{
      name   : "subgraph users",
      script : "index.js",
      cwd: "/home/pekkah/.nvm/versions/node/v24.2.0/lib/node_modules/@apollo/federation-subgraph-compatibility/node_modules/@apollo/federation-subgraph-compatibility-tests/dist/subgraphs/users",
      wait_ready: true
    }]
  }
