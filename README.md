# TwilioFlow2Mermaid
Convert Twilio Flow to Mermaid diagram, which utilize mermaid to make widgets placed automatically in proper place without setting position x and position y.


# Sample call
```
# build or pull docker file
docker build -f Dockerfile -t tf2m:1.1 .

# run docker
docker run -it -v C:\work\TwilioFlow\:/data tf2m:1.1 --files ./giftcard.json ./flow2.json --spreadleafs --endleafs play_tech_difficulties endcall
```
