# Project Libraries Reference

## Overview

This document lists the main libraries and packages used in the AI-Powered CV Screening and Job Match Platform and explains the role of each one in the project.

## Backend Libraries

### 1. ASP.NET Core Web SDK
- Source: `Microsoft.NET.Sdk.Web`
- Role: Provides the core ASP.NET backend framework for building the API, handling routing, controllers, dependency injection, configuration, and HTTP request processing.

### 2. UglyToad.PdfPig
- Version: `1.7.0-custom-5`
- Role: Used in the backend to extract readable text from uploaded PDF files, especially candidate CV documents. This text is then used for parsing, skill extraction, matching, and ranking.

## Frontend Runtime Libraries

### 3. React
- Version: `19.2.4`
- Role: Main frontend UI library used to build reusable components, manage application state, and render the recruiter workflow.

### 4. React DOM
- Version: `19.2.4`
- Role: Connects React components to the browser DOM and enables the frontend application to render in the browser.

## Frontend Build And Development Libraries

### 5. Vite
- Version: `8.0.1`
- Role: Frontend build tool and development server. It provides fast local development, module bundling, and production builds for the React app.

### 6. @vitejs/plugin-react
- Version: `6.0.1`
- Role: Vite plugin that enables React support, including JSX transformation and React-specific development behavior.

## Code Quality And Linting Libraries

### 7. ESLint
- Version: `9.39.4`
- Role: Static analysis tool used to detect code issues, maintain code quality, and promote consistent coding practices in the frontend.

### 8. @eslint/js
- Version: `9.39.4`
- Role: Provides core JavaScript linting rules used by ESLint.

### 9. eslint-plugin-react-hooks
- Version: `7.0.1`
- Role: Ensures correct usage of React Hooks and helps prevent common Hook-related bugs.

### 10. eslint-plugin-react-refresh
- Version: `0.5.2`
- Role: Supports linting rules that work well with React Fast Refresh during development.

### 11. globals
- Version: `17.4.0`
- Role: Provides predefined global variables for linting environments so frontend code can be validated correctly.

## Type Support Libraries

### 12. @types/react
- Version: `19.2.14`
- Role: Provides TypeScript type definitions for React. Even though the current frontend uses JavaScript, this package supports tooling compatibility and future migration.

### 13. @types/react-dom
- Version: `19.2.3`
- Role: Provides TypeScript type definitions for React DOM and supports frontend tooling compatibility.

## Supporting Platform Technologies

These are not additional package libraries in the dependency files, but they are important technologies used by the project:

### 14. .NET 10
- Role: Target runtime for the backend API project.

### 15. OpenAI API
- Role: Used for AI-assisted candidate ranking and explanation generation when the backend is configured with a valid API key.

### 16. PowerShell
- Role: Used in the project for utility scripts such as generating PDF documentation files.

## Summary

The project uses a focused and relatively lightweight technology stack:
- ASP.NET Core for backend API services
- PdfPig for PDF CV text extraction
- React for the frontend interface
- Vite for frontend build tooling
- ESLint and related plugins for code quality
- OpenAI API for AI-assisted ranking functionality

This combination keeps the system modular, understandable, and suitable for an academic, portfolio, or early product prototype.
