'use strict';

var os = require('os');
var http = require('http');

var server = http.createServer(function (request, response) {
  console.log("hit from", request.connection.remoteAddress)
  response.writeHead(200, {"Content-Type": "application/json"});
  response.end(JSON.stringify({
      ip:request.connection.remoteAddress,
      env:process.env,
      net:os.networkInterfaces()
  }));
});

server.listen(3000);

console.log("Server running at http://127.0.0.1:3000/");