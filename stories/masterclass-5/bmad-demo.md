## Scenario 1: Feature Implementation with BMAD Agents

### 1. Install BMAD Method

```bash
npx bmad-method install
```

---

### 2. Initiate Planning with AI Intelligent Help

**Prompt to copy:**

```text
bmad-help We need to add a "Maintenance History" tracking feature for Planes in the AviationApi. Where should we start according to the BMad Method?
```

---

### 3. Architecture Phase (Architect Agent)

**Prompt to copy:**

```text
Let's move to the Architecture phase. Please act as the BMad Architect agent and help me design the MaintenanceRecord model and the corresponding new endpoints in the PlanesController.
```

---

### 4. Implementation Phase (Developer Agent)

**Prompt to copy:**

```text
Act as the BMad Developer agent. Let's implement the MaintenanceRecord model and update the PlanesController and PlaneRepository based on the architecture we just designed.
```

---

### 5. Determine Next Steps

**Prompt to copy:**

```text
bmad-help I just finished the implementation of the Maintenance History feature. What do I do next for testing and deployment?
```

---

## Scenario 2: Party Mode Collaboration

### 1. Trigger Party Mode for a complex problem

**Prompt to copy:**

```text
Let's enter Party Mode! I need the BMad PM, Architect, and Developer agents to discuss how we should handle real-time flight status updates via WebSockets in our .NET AviationApi. Please have them brainstorm and present a unified proposal.
```

---

## Quick reference

| Step            | Terminal (when applicable)      | Chat command (canonical)                   |
| --------------- | ------------------------------- | ------------------------------------------ |
| Install BMAD    | `npx bmad-method install`       | —                                          |
| Non-Interactive | `npx bmad-method install --yes` | —                                          |
| Ask for Help    | —                               | `bmad-help [question or context]`          |
| Call an Agent   | —                               | `Act as the BMad [Role] agent...`          |
| Party Mode      | —                               | `Let's enter Party Mode! [list agents]...` |
