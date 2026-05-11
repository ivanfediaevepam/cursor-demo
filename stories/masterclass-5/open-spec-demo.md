
## Scenario 1: Cross-stack feature alignment

### 1. Initialize OpenSpec and draft a proposal
```bash
openspec --version
openspec init
```
**Prompt to copy:**

```text
/opsx-propose We need to add a "Delay Reason" feature. When a flight's status is changed to "Delayed", users should be able to provide a reason (e.g., "Weather", "Technical"). This needs to be stored in the .NET backend and displayed on the React frontend.
```
---
### 2. Review and refine the proposal
**Prompt to copy:**

```text
This looks good, but let's make sure the delay reason in the .NET model is optional (nullable), since it only applies when the status is "Delayed". Please update the proposal tasks to reflect this.
```
---

### 3. Apply the specification
**Prompt to copy:**

```text
/opsx-apply Proceed with implementing the tasks outlined in this spec.
```
---

### 4. Archive the completed change
**Prompt to copy:**
```text
/opsx-archive The Delay Reason feature is complete and tested.
```
---

## Quick reference

| Step               | Terminal (when applicable)                                             | Chat command (canonical)                      |
| ------------------ | ---------------------------------------------------------------------- | --------------------------------------------- |
| Install CLI        | `npm install -g @fission-ai/openspec@latest` then `openspec --version` | —                                             |
| Init repo          | `cd <repo-root> && openspec init`                                      | —                                             |
| New change         | —                                                                      | `/opsx-propose …`                             |
| Implement          | —                                                                      | `/opsx-apply …`                               |
| Finish             | —                                                                      | `/opsx-archive …`                             |
