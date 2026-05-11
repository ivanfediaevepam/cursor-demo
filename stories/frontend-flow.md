# Planes list — React component hierarchy

Mermaid **flowchart** (`flowchart TB`) for the aviation collection list: composition, static data from the page, React Router usage, and how the detail route uses `PlaneService` after leaving the list.

**Data note:** The list is fed by a **static array** in `HomePage`; `PlaneService` is not used to load the list (only `PlaneDetail` calls it).

```mermaid
flowchart TB
  subgraph RRD["react-router-dom"]
    NavAPI["useNavigate · Routes · Route"]
  end

  subgraph App["App.tsx"]
    Router["BrowserRouter"]
    Routes["Routes"]
    RouteHome["Route / → HomePage"]
    RouteDetail["Route /planes/:planeId → PlaneDetail"]
    Router --> Routes
    Routes --> RouteHome
    Routes --> RouteDetail
  end

  subgraph HP["HomePage.tsx"]
    Banner["Banner"]
    Air["Airplane"]
    Prop["PropellerSVG"]
    PageContent["PageContent"]
    StaticData["planes array static in HomePage"]
    Banner --> Air
    Banner --> Prop
    Banner --> PageContent
  end

  subgraph PL["PlaneList.tsx"]
    PLRoot["div.planes-list-container"]
    H2["h2 title"]
    UL["ul"]
    LI["li per plane"]
    IMG["img plane.png"]
    Meta["div name + year"]
    PLRoot --> H2
    PLRoot --> UL
    UL --> LI
    LI --> IMG
    LI --> Meta
  end

  subgraph Detail["PlaneDetail detail route"]
    PD["PlaneDetail"]
    PS["PlaneService axios"]
    PD --> PS
  end

  RouteHome --> HP
  PageContent --> PLRoot
  StaticData -->|"props planes"| PLRoot

  PL -.->|"useNavigate"| NavAPI
  NavAPI -.-> Router
  LI -.->|"after timeout"| PD

  RouteDetail --> PD
```

## Legend

| Part | Role |
|------|------|
| **App / Routes** | `BrowserRouter` wraps `Routes`; `/` renders the list page; `/planes/:planeId` renders the detail page. |
| **HomePage** | Defines static `planes`, renders `Banner` (with `Airplane`, `PropellerSVG`), then `PageContent` wrapping `PlaneList`. |
| **PageContent** | Layout wrapper only; forwards `children` (`PlaneList`). |
| **PlaneList** | Renders heading, `ul`, and one `li` per plane with thumbnail and text; calls `useNavigate` and `navigate(/planes/${id})` after the fly-away delay. |
| **Dashed edges** | Imperative navigation and hook usage, not React parent/child tree. |
| **PlaneDetail / PlaneService** | Shown because the list navigates here; **not** a child of `PlaneList`. |

## Files

| Concern | Path |
|---------|------|
| Routes | `src/frontend/src/App.tsx` |
| List page + data | `src/frontend/src/pages/HomePage.tsx` |
| List UI | `src/frontend/src/components/PlaneList.tsx` |
| Layout / header | `src/frontend/src/components/PageContent.tsx`, `Banner.tsx`, `Airplane.tsx` |
| API on detail | `src/frontend/src/services/PlaneService.ts` |
