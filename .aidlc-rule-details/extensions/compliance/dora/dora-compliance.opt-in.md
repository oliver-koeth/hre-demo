# DORA Compliance Documentation — Opt-In

**Extension**: DORA Compliance Documentation

## Opt-In Prompt

The following question is automatically included in the Requirements Analysis clarifying questions when this extension is loaded:

```markdown
## Question: DORA Compliance Documentation Extension
Should DORA compliance documentation rules be enforced for this project?

**What this extension is.** Enabling it requires a continuously maintained DORA compliance document populated from repository evidence (CI workflows, tests, traceability artifacts, and governance docs), with explicit scope boundaries for development/local integration environments.

**What this extension is NOT.** Enabling it does not claim full production or operational DORA compliance by itself. Any operationally owned sections must explicitly state: "will be provided by operations".

A) Yes — enforce DORA documentation rules as blocking constraints, including freshness checks and integrity stamp requirements

B) No — skip DORA documentation rules

X) Other (please describe after [Answer]: tag below)

[Answer]:
```
