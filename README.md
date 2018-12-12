# Database
### Technology: DynamoDB

###  Design and Logic
- SessionFeedbacks table (supporting concurrent writes and answering query: last feedbacks for session X with rate Y)
It is used to store all feedbacks. The best HASH (partition) key we could use for this table could be SessionId_UserId since it is unique so it would be a good way to distribute the load but since we had the requirement to make a query and retrieve last feedbacks for a specific SessionId with a specific rate, we had to use SessionId as primary key(not the best but still a good way to distribute writes to different partitions in case we think the number of concurrent users per session is limited).
UserId is used as the main rang key so the combination of SessionId+UserId is unique in our SessionFeedbacks table and we can do a conditional update on our table and prevent a user to send multiple feedbacks for a session.
To be able to do an order by a query for a specific SessionId based on the date we used SubmitDate as our LocalSeconadryIndex (meaning adding an index on SubmitDate locality per sessionId, it is like having SessionId and SubmitDate as hash and range).
Since we needed to retrieve last feedbacks with rate X for a specific SessionId, we used combination of sessionId and rate (sessionId_rate) to have our Global Secondary Index (GIS is like main indices composed of HASH and RANGE key but do not have a requirement to be unique) and again SubmitDate as our range key which lets use to make a query like getting last 15 feedbacks with sessionId XXX having rate YYY. 
As we discussed in the beginning if we are worried about the number of concurrent users per session posting feedbacks and we wanted to have a better distribution we could use sessionId_userId  as our partition key for this table ( supporting writes concurrent writes) and use a logic like below section to retrieve the last feedbacks for a session with a specified rate.
In this test, I used the simple version for Sessionfeedbacks and more eventual approach for supporting lastfeedbacks across all sessions query as it is explained in the next section.

- LastUserFeedbacks table  (answering query: last feedbacks with rate X)
To be able to make a query across all sessions for a specific rate and retrieve last feedbacks, we used another table with a constant number of writes. Since we need to use rate (number from 1 to 5) there is not a good way to distribute write loads for all userfeedbacks, by limiting the writes for last 15 users' feedbacks during last 10 seconds on each server, we will get a manageable number of writes per server and the distribution of writes are not that important since we would have numberofservers*15*6 writes per minute on our table, we could use a simple table (without being worried about concurrent writes in high scale).
In this table rate used as one index and SubmitDate is used another index lets us make a query to answer last feedbacks across all session with rate X.
It must be noted this table might have a 10 seconds delay to report last feedbacks, meaning per request the result might be delayed 10 seconds.
To implement this logic each server first will add a record to SessionFeedbacks then will cache the write in a dictionary in its memory and every 10 seconds it will run a background task to store its last 15 records to the database and flush its cache.

# API Documentation and Running
It is a aspnet core app so run applicationas as:

After runing the application  browse to .../swagger/index.html you will find the API.

# How to run
* Download AWS CLI and install (https://aws.amazon.com/cli/) 
Configure CLI and set a fake default profile (Keys not important just put xxx)
```sh
$ aws configure
AWS Access Key ID [****************xxx]:XXX
AWS Secret Access Key [****************xxx]:XXX
Default region name [eu-west-1]:eu-west-1
Default output format [None]:
```
* Download localdyanomdb(https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.DownloadingAndRunning.html
And extract it then open shell go to the extracted folder and run below line to start the process  

```sh
$ java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar 
```
This command will run dynamodb locally on port 8000 port 8000 is used inside application as default so make sure you use this port number or if you changed the port (by adding -port XXXX to the command ) you need  to change port number in applicationsetting.json.
Finally, open another shell browse to website folder you have two json file the name tables. You need to run below commands to create tables.
```sh
$ aws dynamodb create-table --cli-input-json file://sessionfeedbacks.json --endpoint-url http://localhost:8000
$ aws dynamodb create-table --cli-input-json file://lastuserfeedbacks.json --endpoint-url http://localhost:8000
```
Finally, you can run the Asp.net core app and by browsing to localhost:XXXX/swagger/index.html you can see the APIs documentation and you can run the APIs. 

