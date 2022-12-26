This project listens on specific host and port (these are configured through serviceConfig.json file) and serves upcoming HTTP requests. It uses the queue pattern to, actually, queue received messages and process them in a separate thread. If a received message contains "GET / HTTP/1.1" (the request for an HTML page) or "GET /favicon.ico HTTP/1.1" (the request for favicon), then the service sends the respective data with status = "HTTP/1.1 200 OK", otherwise the request message is treated as an invalid one, thus the service sends back the "HTTP/1.1 400 Bad Request" status only with no other data.
