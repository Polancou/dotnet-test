# Frontend Client (Vue 3)

The frontend application built with **Vue 3**, **Vite**, and **TypeScript**. It serves as the user interface for interacting with either the .NET or Python backend.

## 🚀 Technologies

-   **Framework**: Vue 3
-   **Build Tool**: Vite
-   **State Management**: Pinia
-   **Styling**: Tailwind CSS
-   **Language**: TypeScript

## 🛠️ Setup & Run

### Prerequisites
-   Node.js (v18+)
-   npm
-   Docker (optional)

### 1. Environment Configuration

1.  Navigate to the `client` directory.
2.  Copy the example environment file:
    ```bash
    cp .env.example .env
    ```
3.  Open `.env` and configure your API URL (default is `http://localhost:5000` for .NET, `http://localhost:8001` for Python).

### 2. Local Execution

1.  **Install dependencies:**
    ```bash
    npm install
    ```

2.  **Start the development server:**
    ```bash
    npm run dev
    ```
    - App: `http://localhost:5173`

3.  **Build for Production:**
    ```bash
    npm run build
    ```
    Output: `dist/` directory.

### 3. 🐳 Docker Execution

1.  **Build the image:**
    ```bash
    docker build -t client-app .
    ```

2.  **Run the container:**
    ```bash
    docker run -d -p 80:80 --name client-app client-app
    ```
    - App: `http://localhost:80`

## ⚙️ Configuration

By default, the client is configured to communicate with the backend. Check `.env` files if you need to switch between the .NET backend (default port often 5000/5001) and the Python backend (port 8001).
