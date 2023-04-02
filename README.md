# Advent

**Advent** is a ⚡️quick start kit ⚡️for [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel).

Advent automatically discovery both semantic and native skills from a folder, and exposes all the discovery skills via a
REST API. And Advent uses [Qdrant](https://github.com/qdrant/qdrant) as the persistent semantic memory.

### Get Started

---

#### Install


Using your favorite Node.js package management tool(mine is pnpm), run:

```pnpm
pnpm i advent-ai
```

#### API server


To use Advent API server for you awesome AI app, you can put your semantic and native skills
under the `skills` folder. And start the Advent API. 

Make sure you have .NET Core and Qdrant installed, then run the following command:

```npm
npx advent api
```

The API of the server is really simple:

- List all available skills as HAL JSON
    - `GET` https://localhost:6666/api/skills
- Get detailed skill description
    - `GET` https://localhost:6666/api/skills/{skill}/{function}
- Execute functions
    - `POST` https://localhost:7071/api/asks?iterations={iterations}

In order to execute functions, the following JSON must be provided.

```json
{
  "variables": [
    {
      "key": "INPUT",
      "value": "...."
    }
  ],
  "pipeline": [],
  "skills": []
}
```

**variables** is an array of kay value pair, for the input to the kernel.

**pipeline** is the chained or piped functions would like to run. For example, the following json will
run `TextSkill.Uppercase` and `TextSkill.TrimEnd` as piped functions:

```json
{
  "variables": [
    {
      "key": "INPUT",
      "value": " lowercase"
    }
  ],
  "pipeline": [
    {
      "skill": "TextSkill",
      "name": "Uppercase"
    },
    {
      "skill": "TextSkill",
      "name": "TrimEnd"
    }
  ],
  "skills": []
}
```

If no functions specified, it will run `PlannerSkill.CreatePlan` and `PlannerSkill.ExecutePlan` by default (a.k.a, archive goal). 
And the *iterations* query parameter will be used to determine how many times should the kernel try before the plan execute successfully.

*skills* indicates which skills will be used during the execution. Since the planner tend to use
most of the available functions, the result plan might be too long. And can't fit within the token limits.
Then skills could be used to tell the kernel exactly which skills should the plan be based on. 

Every API call should provide OpenAI API key via HTTP headers:

| Header                       |                                         |
|------------------------------|-----------------------------------------|
| x-advent-text-completion-key | OpenAI API key for text completion      |
| x-advent-chat-completion-key | OpenAI API key for chat completion      |
| x-advent-embedding-key       | OpenAI API key for embedding generation |

And if you use the same key for different purposes, you only need to provide `x-advent-text-completion-key`.

#### Embeddings indexing

Semantic memory with "embeddings" is growing in popularity when a set of documents needed to be provide for LLM. 
To index documents, run the following command:

```npm
npx advent index <path to folder> -c <collection name> -i .md .txt 
```

That's it. Have fun with AI 🧗‍!

License

---

Advent is licensed under the MIT License.

