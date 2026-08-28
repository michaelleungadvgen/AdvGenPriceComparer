# Frontend Web Architecture Plan: Grocery Price Comparer

This document outlines the architectural design for a React-based frontend web application designed to query, display, and compare grocery prices from a JSON data source (e.g., `price_comparison_data.json`).

## 1. Technology Stack

*   **Framework:** React 18+ (using Functional Components and Hooks)
*   **Build Tool/Framework:** Next.js (recommended for routing, SSR/SSG capabilities, and API routes if needed) or Vite (for a fast Single Page Application). *We will assume Next.js for this design.*
*   **Language:** TypeScript (for type safety and better developer experience).
*   **Styling:** Tailwind CSS (for rapid, utility-first styling) + Shadcn UI (for accessible, pre-built components like tables, cards, and dropdowns).
*   **State Management:** React Context API (for global UI state like themes/filters) + React Query (TanStack Query) for data fetching, caching, and state management of the JSON data.
*   **Data Fetching:** Standard `fetch` API wrapped by React Query.
*   **Icons:** Lucide React.

## 2. Core Features

1.  **Dashboard/Overview:** Display summary statistics (total products, active stores, last updated date) extracted from the JSON `metadata`.
2.  **Category Browsing:** Browse products by category (e.g., "Premium Chocolate", "Fresh Meat & Seafood").
3.  **Product Listing & Filtering:** Display a list of products with the ability to filter by store (Coles, Woolworths, Drakes), brand, and "on special" status.
4.  **Price Comparison View:** A dedicated view highlighting products that exist in multiple stores to show direct price comparisons (utilizing the `price_comparisons` array in the JSON).
5.  **Search:** Global text search across product names and descriptions.

## 3. Data Structure Analysis (Based on `price_comparison_data.json`)

The application needs to handle the following JSON structure:
*   `metadata`: Contains `generated_date`, `total_products`, `stores` list, and `last_updated`.
*   `categories`: An object keyed by category name. Each category contains:
    *   Counts (`total_products`, `coles_count`, etc.)
    *   `products`: An object keyed by store name (e.g., `coles`, `woolworths`), containing arrays of product objects.
*   `price_comparisons`: An object keyed by category name, containing arrays of comparison objects showing `similarity_score`, `price_difference`, `cheaper_store`, and the individual product objects from the respective stores.
*   `summary`: High-level counts per category.

## 4. Component Architecture

### 4.1. Layout Components
*   **`Layout`**: The main wrapper, including the `Header`, `Sidebar` (navigation/filters), and `MainContent` area.
*   **`Header`**: Contains the app logo, global search bar, and last updated timestamp.
*   **`Sidebar`**: Navigation links (Dashboard, Categories, Best Deals) and global filters (Store toggles).

### 4.2. UI Components (Shared)
*   **`ProductCard`**: Displays individual product details (Name, Brand, Current Price, Original Price, Discount Badge, Store Logo).
*   **`ComparisonCard`**: A specialized card showing the same/similar product side-by-side from different stores, highlighting the cheaper option.
*   **`Badge`**: For displaying tags like "Half Price", store names, or categories.
*   **`SearchBar`**: Reusable text input for filtering.

### 4.3. Page Components (Next.js Routes)

#### `/` (Dashboard)
*   Displays the `SummaryStats` component (using `metadata` and `summary`).
*   Shows a "Top Deals" or "Recent Comparisons" section.

#### `/products` (All Products / Category View)
*   **`CategorySelector`**: Dropdown or tabs to switch between categories.
*   **`FilterPanel`**: Controls for Store, Brand, and "On Special Only".
*   **`ProductGrid`**: Renders a grid of `ProductCard` components based on the selected category and filters. Must handle flattening the nested store arrays within a category into a single list for rendering, or grouping them visually.

#### `/compare` (Direct Comparisons)
*   Specifically reads from the `price_comparisons` object in the JSON.
*   Renders a list of `ComparisonCard` components.
*   Allows filtering comparisons by category or "biggest price difference".

## 5. State Management & Data Fetching

### 5.1. Data Fetching Strategy (React Query)
Since the data is a static JSON file (which might be updated periodically by a backend process), we will use React Query to fetch and cache it.

```typescript
// hooks/usePriceData.ts
import { useQuery } from '@tanstack/react-query';

const fetchPriceData = async () => {
  const response = await fetch('/api/data'); // Or direct path if hosted statically
  if (!response.ok) throw new Error('Network response was not ok');
  return response.json();
};

export const usePriceData = () => {
  return useQuery({
    queryKey: ['priceData'],
    queryFn: fetchPriceData,
    staleTime: 1000 * 60 * 5, // Cache for 5 minutes
  });
};
```

### 5.2. Derived State Hooks
We will create custom hooks to process the raw JSON into usable formats for specific components.

*   `useCategories()`: Extracts the list of category names.
*   `useProductsByCategory(categoryName)`: Flattens the store-separated products within a category into a single array for rendering the grid.
*   `useComparisons()`: Returns the flat list of `price_comparisons`.

### 5.3. UI State (Context)
A simple React Context to manage user preferences:
*   `selectedStores`: Array of currently selected stores for filtering (e.g., `['Coles', 'Woolworths']`).
*   `searchQuery`: The current global search text.

## 6. Implementation Steps

1.  **Initialize Project:** `npx create-next-app@latest grocery-comparer --typescript --tailwind`
2.  **Install Dependencies:** `npm install @tanstack/react-query lucide-react` (plus Shadcn UI setup).
3.  **Setup Types:** Create TypeScript interfaces defining the JSON structure (`Metadata`, `Product`, `Pricing`, `Category`, `Comparison`).
4.  **Create API Route (Mock):** If using Next.js, place `price_comparison_data.json` in a secure location and create an API route (`/pages/api/data.ts` or App Router equivalent) to serve it.
5.  **Build Shared Components:** Develop `ProductCard`, `Badge`, layout elements.
6.  **Implement Data Hooks:** Create `usePriceData` and derived hooks.
7.  **Build Pages:** Assemble the Dashboard, Products page, and Comparisons page.
8.  **Implement Filtering:** Connect the Context state to the rendering logic in the `ProductGrid`.

## 7. Future Enhancements
*   **Pagination/Virtualization:** If the JSON file grows very large, implement infinite scrolling or windowing (e.g., `@tanstack/react-virtual`) to maintain performance.
*   **Historical Charts:** Integrate a charting library (like Recharts) to plot price history if that data is added to the JSON.
*   **Shopping List:** Allow users to "add to cart/list" to calculate total trip costs across different stores.
