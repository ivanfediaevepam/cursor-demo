# Cursor Skills Demo Script: "The Flight Status Story"

This document provides a single, end-to-end workflow demonstrating all 11 custom Cursor Skills. Instead of isolated tricks, we will build, test, document, and review a complete "Flight Status" feature from backend API down to the frontend UI components.

Run these demos sequentially. For each step, open the target file, open Cursor Chat (`Cmd+L`), and paste the exact prompt provided.

---

## 1. Generate New API

**Goal:** Create the backend endpoint for our new feature.
**Context:** Open `FlightsController.cs`.

**Prompt to copy:**
```text
/generate-api Add a PUT /flights/{id}/status endpoint to update a flight's status. It should check if the flight exists (returning 404 if not), update the status, and return 200 OK with the updated flight.
```

---

## 2. Test Generation for .NET

**Goal:** Ensure our new endpoint works correctly.
**Context:** Open `FlightsController.cs` (focusing on the new method).

**Prompt to copy:**
```text
/test-generation-dotnet Generate a full xUnit test class for FlightsController. Cover the new PUT status method specifically, testing both the successful update scenario and the 404 not found scenario.
```

---

## 3. Update Documentation

**Goal:** Add standard inline documentation.
**Context:** Open `FlightsController.cs`.

**Prompt to copy:**
```text
/update-documentation Our backend code is missing inline documentation. Please add standard XML summary comments to the new PUT method, explaining the parameters and what it returns.
```

---


## 5. Create Diagrams

**Goal:** Visualize the new backend architecture.
**Context:** Do not open any specific file (let it use workspace context).

**Prompt to copy:**
```text
/create-diagrams Generate a Mermaid sequence diagram illustrating the data flow when a user calls the new PUT /flights/{id}/status endpoint. Show the interaction between FlightsController, FlightRepository, and the Model. Save it to stories/status-flow.md.
```

---

## 6. Create Diagrams - Frontend Component Hierarchy

**Goal:** Visualize the frontend component structure and data flow.
**Context:** Open `PlaneList.tsx`.

**Prompt to copy:**
```text
/create-diagrams Generate a Mermaid flowchart showing the React component hierarchy for the planes list. Include the PlaneList component, any child items, and the routing or data fetching services it interacts with. Save it to stories/frontend-flow.md.
```

---

## 7. Create Diagrams - System Architecture

**Goal:** Visualize the overall high-level architecture of the application.
**Context:** Workspace context.

**Prompt to copy:**
```text
/create-diagrams Generate a Mermaid C4-style deployment or architecture diagram showing the entire Aviation Demo system. Include the React Vite frontend, the ASP.NET Core backend, and the in-memory repository layer. Save it to stories/system-architecture.md.
```

---

## 8. Generate New React Component

**Goal:** Build the frontend UI to display the status.
**Context:** Open `PlaneList.tsx` (so the AI sees current styling patterns).

**Prompt to copy:**
```text
/generate-react-component Create a new FlightStatusBadge.tsx component. It should take a `status` prop (using the FlightStatus enum). Render a styled pill badge where the background color changes based on the status (e.g., green for Landed, yellow for Delayed). Match the Tailwind aesthetic of PlaneList.
```

---

## 9. Generate Storybook Story

**Goal:** Isolate and visually test the new UI component.
**Context:** Open the newly created `FlightStatusBadge.tsx`.

**Prompt to copy:**
```text
/generate-storybook-story Generate a complete Storybook file for this component. I want a story for every single possible FlightStatus so we can visually test all the color variations in isolation.
```

---

## 10. Test Generation for JS

**Goal:** Add automated DOM tests for our component.
**Context:** Open `FlightStatusBadge.tsx`.

**Prompt to copy:**
```text
/test-generation-js Generate a Vitest and React Testing Library spec file for FlightStatusBadge.tsx. Write tests to ensure it correctly renders the text and applies the right Tailwind classes for at least two different statuses.
```

---

## 11. Bug Fix

**Goal:** Fix a (simulated) issue in our new feature.
**Context:** Open `FlightStatusBadge.tsx`.

**Prompt to copy:**
```text
/bug-fix There is a bug: when the status is "Cancelled", the text color in the badge is hard to read against the red background. Find the issue in the Tailwind classes, fix it safely to ensure good contrast, and update the related test if necessary.
```

---

## 12. Tools Generation

**Goal:** Prepare the feature for CI/CD.
**Context:** Workspace context.

**Prompt to copy:**
```text
/generate-github-action We need a CI pipeline for our new full-stack feature. Create a GitHub Actions workflow that triggers on pull requests to main. Run two parallel jobs: build/test the .NET backend, and npm install/build for the React frontend.
```

---