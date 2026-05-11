/// <reference types="node" />
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer(
  {
    name: "backstage-bridge",
    version: "1.0.0",
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

const catalogSchemasText = `
Corporate Standards for Backstage YAMLs

For Components:
\`\`\`yaml
apiVersion: backstage.io/v1alpha1
kind: Component
metadata:
  name: <component-name>
  description: <A brief description of what the component does>
  tags:
    - <tag1>
    - <tag2>
  annotations:
    github.com/project-slug: <org>/<repo>
    backstage.io/techdocs-ref: dir:.
spec:
  type: <service | library | website>
  lifecycle: <production | experimental>
  owner: group:<squad-name>
  providesApis:
    - <api-name>
  consumesApis:
    - <api-name>
  dependsOn:
    - component:<component-name>
\`\`\`

For APIs:
\`\`\`yaml
apiVersion: backstage.io/v1alpha1
kind: API
metadata:
  name: <api-name>
  description: <A brief description of the API>
  tags:
    - <tag1>
  annotations:
    github.com/project-slug: <org>/<repo>
spec:
  type: <openapi | asyncapi | graphql>
  lifecycle: <production | experimental>
  owner: group:<squad-name>
  definition: |
    $text: https://raw.githubusercontent.com/<org>/<repo>/main/openapi.yaml
    # OR inline definition string
\`\`\`
`;

server.registerTool(
  "get_catalog_schemas",
  {
    description:
      "Returns the strict corporate standards for Backstage YAMLs. Enforces official Backstage specification for Component and API kinds.",
  },
  async () => ({
    content: [{ type: "text", text: catalogSchemasText }],
  })
);

const mockCatalog: Record<
  string,
  {
    apiVersion: string;
    kind: string;
    metadata: Record<string, unknown>;
    spec: Record<string, unknown>;
  }
> = {
  "billing-service": {
    apiVersion: "backstage.io/v1alpha1",
    kind: "Component",
    metadata: {
      name: "billing-service",
      description: "Handles customer billing and invoicing",
      tags: ["java", "spring-boot", "finance"],
      annotations: {
        "github.com/project-slug": "myorg/billing-service",
      },
    },
    spec: {
      type: "service",
      lifecycle: "production",
      owner: "group:finance-squad",
      providesApis: ["billing-api"],
      consumesApis: ["payment-gateway"],
      dependsOn: ["component:user-service"],
    },
  },
  "payment-gateway": {
    apiVersion: "backstage.io/v1alpha1",
    kind: "API",
    metadata: {
      name: "payment-gateway",
      description: "External payment gateway API wrapper",
    },
    spec: {
      type: "openapi",
      lifecycle: "production",
      owner: "group:payments-squad",
      definition:
        "$text: https://github.com/myorg/payment-gateway/raw/main/openapi.yaml",
    },
  },
};

server.registerTool(
  "fetch_service_context",
  {
    description:
      "Fetches a mock JSON object simulating an existing Backstage catalog entity to understand current dependencies.",
    inputSchema: {
      serviceName: z
        .string()
        .describe("The name of the service to fetch context for"),
    },
  },
  async (args) => {
    const entity =
      mockCatalog[args.serviceName] ?? {
        error:
          "Service not found in catalog. It might be new and require scaffolding.",
      };
    return {
      content: [{ type: "text", text: JSON.stringify(entity, null, 2) }],
    };
  }
);

const techDocsTemplate = `---
id: \${1:doc-id}
title: \${2:Document Title}
description: \${3:Document Description}
---

# \${2:Document Title}

\${3:Document Description}

## Architecture Diagram

<!-- AI: Insert Mermaid architecture diagram here -->
\`\`\`mermaid
graph TD;
    A-->B;
\`\`\`

## API Definitions

<!-- AI: Describe the main endpoints or API interactions here -->
- **Endpoint**: 
- **Description**: 

## Environment Variables

<!-- AI: List required environment variables and their purposes -->
| Variable | Description | Required |
|----------|-------------|----------|
| \`ENV_VAR\` | Details | Yes/No |

`;

server.registerTool(
  "get_techdocs_template",
  {
    description:
      "Returns a standardized Markdown template for internal TechDocs.",
  },
  async () => ({
    content: [{ type: "text", text: techDocsTemplate }],
  })
);

async function run() {
  console.error("Starting Backstage MCP Server on stdio...");
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("Backstage MCP Server running and ready to handle requests.");
}

run().catch((error) => {
  console.error("Fatal error running MCP server:", error);
  process.exit(1);
});
