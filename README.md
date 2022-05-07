# TwilioFlow2Mermaid
Convert Twilio Flow to Mermaid diagram, but only widget names kept, a lot information is adandomed, like positions.
WHY: 
- For a complex flow, it would a night-mare to maintain the positions of widgets in Twilio Studio.
- Leverage mermaid to display the flow as diagram automatically.

## Sample call
```
# build or pull docker file
docker build -f Dockerfile -t tf .

# docker run
docker run -it --rm -v C:\work\TwilioFlow\:/data tf --help

# docker run
docker run -it --rm -v C:\work\TwilioFlow\:/data tf --files ./giftcard.json ./flow2.json --spreads play_tech_difficulties endcall --nospreads false

## remove dangling images
```shell
# docker image ls -f dangling=true
docker image prune
```

## docker push
```shell
docker tag tf wizardlsw/twilioflow2mermaid:0.1
docker tag tf wizardlsw/twilioflow2mermaid:latest

docker push wizardlsw/twilioflow2mermaid:0.1
docker push wizardlsw/twilioflow2mermaid:latest
```

# Generate files
Here we use another image to do this, which is [mermaid-cli]:
```
docker pull minlag/mermaid-cli

docker run -it --rm -v /path/to/diagrams:/data minlag/mermaid-cli -i /data/diagram.md
```

[mermaid-cli]: <https://hub.docker.com/r/minlag/mermaid-cli>


