import React from 'react';
import ReactDOM from 'react-dom/client';
import UrlShortenerApp from './UrlShortenerApp.jsx';

// This function will be called from the Razor view
window.initializeUrlShortenerApp = function(props) {
    const rootElement = document.getElementById('url-shortener-root');
    
    if (!rootElement) {
        console.error('Root element #url-shortener-root not found');
        return;
    }
    
    try {
        const root = ReactDOM.createRoot(rootElement);
        root.render(
            React.createElement(UrlShortenerApp, props)
        );
        console.log('React app initialized successfully');
    } catch (error) {
        console.error('Error initializing React app:', error);
    }
};

