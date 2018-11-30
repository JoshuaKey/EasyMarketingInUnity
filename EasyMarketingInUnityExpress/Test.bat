
::SET Data="{\"id\": \"t3_8pbi01\", \"dir\": -1}"
SET Data='{"text":"Hello, World!"}'

SET ConType="Content-Type: application/json"
SET Auth="Authorization: Bearer 11799428-kjjABG9hSYpHLEB06a5j49dRuSo"
SET Agent="User-Agent: PC:com.example.EasyMarketingInUnity:v0.1 (by /u/Flameo326)"

SET Base=https://hooks.slack.com/services/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
SET Endpoint=
SET Query=

SET URL="%Base%%Endpoint%%Query%"

curl -v -d %Data% -H %ConType% %URL% || PAUSE
::-H %Agent% -H %Auth%
@pause

::curl -X POST -H 'Content-type: application/json' --data '{"text":"Hello, World!"}' https://hooks.slack.com/services/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