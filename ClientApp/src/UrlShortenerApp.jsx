import { useState, useEffect } from 'react';

const UrlShortenerApp = ({ initialUrls, isAuthenticated, isAdmin, currentUser, baseUrl }) => {
    const [urls, setUrls] = useState(initialUrls || []);
    const [newUrl, setNewUrl] = useState('');
    const [loading, setLoading] = useState(false);
    const [errorMessage, setErrorMessage] = useState('');
    const [deleting, setDeleting] = useState({});

    // Function to fetch updated URLs from server
    const fetchUrls = async () => {
        try {
            const response = await fetch('/ShortUrl/GetAllUrls');
            if (response.ok) {
                const data = await response.json();
                setUrls(data);
            }
        } catch (error) {
            console.error('Error fetching URLs:', error);
        }
    };

    // Update URLs when window gains focus (user returns from redirect)
    useEffect(() => {
        const handleFocus = () => {
            fetchUrls();
        };

        window.addEventListener('focus', handleFocus);
        return () => {
            window.removeEventListener('focus', handleFocus);
        };
    }, []);

    // Update URLs when page becomes visible (user returns to tab)
    useEffect(() => {
        const handleVisibilityChange = () => {
            if (!document.hidden) {
                fetchUrls();
            }
        };

        document.addEventListener('visibilitychange', handleVisibilityChange);
        return () => {
            document.removeEventListener('visibilitychange', handleVisibilityChange);
        };
    }, []);

    // Periodic update every 30 seconds
    useEffect(() => {
        const interval = setInterval(() => {
            fetchUrls();
        }, 30000); // Update every 30 seconds

        return () => {
            clearInterval(interval);
        };
    }, []);

    const canDelete = (url) => {
        if (!isAuthenticated) return false;
        if (isAdmin) return true;
        return url.createdBy === currentUser;
    };

    // Auto-hide error message after 3 seconds
    useEffect(() => {
        if (errorMessage) {
            const timer = setTimeout(() => {
                setErrorMessage('');
            }, 3000);

            return () => {
                clearTimeout(timer);
            };
        }
    }, [errorMessage]);

    // Clear error message when user starts typing
    const handleUrlChange = (e) => {
        setNewUrl(e.target.value);
        if (errorMessage) {
            setErrorMessage('');
        }
    };

    const addUrl = async (e) => {
        e.preventDefault();
        if (!newUrl || loading) return;

        setLoading(true);
        setErrorMessage('');

        try {
            const response = await fetch('/ShortUrl/CreateShortUrl', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ originalUrl: newUrl })
            });

            const data = await response.json();

            if (data.success) {
                setUrls(prevUrls => [data.data, ...prevUrls]);
                setNewUrl('');
                setErrorMessage('');
            } else {
                setErrorMessage(data.message || 'Error creating short URL');
            }
        } catch (error) {
            setErrorMessage('Error creating short URL. Please try again.');
            console.error('Error:', error);
        } finally {
            setLoading(false);
        }
    };

    const deleteUrl = async (id) => {
        if (deleting[id] || !window.confirm('Are you sure you want to delete this URL?')) {
            return;
        }

        setDeleting(prev => ({ ...prev, [id]: true }));

        try {
            const response = await fetch(`/ShortUrl/Delete?id=${id}`, {
                method: 'DELETE',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            const data = await response.json();

            if (data.success) {
                setUrls(prevUrls => prevUrls.filter(url => url.id !== id));
            } else {
                alert(data.message || 'Error deleting URL');
            }
        } catch (error) {
            alert('Error deleting URL. Please try again.');
            console.error('Error:', error);
        } finally {
            setDeleting(prev => ({ ...prev, [id]: false }));
        }
    };

    return (
        <div className="url-shortener-container">
            <div className="page-header">
                <h1 className="page-title">
                    <i className="bi bi-link-45deg"></i> Short URLs
                </h1>
                <a href="/About/Index" className="btn btn-outline-primary">
                    <i className="bi bi-info-circle"></i> About
                </a>
            </div>

            {isAuthenticated && (
                <div className="add-url-card">
                    <div className="card-header-custom">
                        <h5 className="mb-0">
                            <i className="bi bi-plus-circle"></i> Add New URL
                        </h5>
                    </div>
                    <div className="card-body-custom">
                        <form onSubmit={addUrl}>
                            <div className="add-url-form">
                                <div className="url-input-wrapper">
                                    <input
                                        type="url"
                                        className="form-control url-input"
                                        value={newUrl}
                                        onChange={handleUrlChange}
                                        placeholder="Enter URL (e.g., https://example.com)"
                                        required
                                        disabled={loading}
                                    />
                                    <button
                                        type="submit"
                                        className="btn btn-primary shorten-btn"
                                        disabled={loading}
                                    >
                                        {loading ? (
                                            <>
                                                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                                                Processing...
                                            </>
                                        ) : (
                                            <>
                                                <i className="bi bi-scissors"></i> Shorten
                                            </>
                                        )}
                                    </button>
                                </div>
                                {errorMessage && (
                                    <div className="alert alert-danger mt-3 mb-0" role="alert">
                                        <i className="bi bi-exclamation-triangle"></i> {errorMessage}
                                    </div>
                                )}
                            </div>
                        </form>
                    </div>
                </div>
            )}

            <div className="urls-table-wrapper">
                {urls.length === 0 ? (
                    <div className="empty-state">
                        <i className="bi bi-inbox empty-icon"></i>
                        <h3>No URLs found</h3>
                        <p>{isAuthenticated ? 'Add your first URL above to get started!' : 'Login to add URLs.'}</p>
                    </div>
                ) : (
                    <div>
                        <table className="table table-hover urls-table">
                            <thead>
                                <tr>
                                    <th><i className="bi bi-link-45deg"></i> Original URL</th>
                                    <th><i className="bi bi-shortcut"></i> Short URL</th>
                                    <th><i className="bi bi-person"></i> Created By</th>
                                    <th><i className="bi bi-calendar"></i> Created Date</th>
                                    <th><i className="bi bi-mouse"></i> Clicks</th>
                                    <th><i className="bi bi-gear"></i> Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {urls.map((url) => (
                                    <tr key={url.id} className="url-row">
                                        <td>
                                            <a
                                                href={url.originalUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="original-url-link"
                                                title={url.originalUrl}
                                            >
                                                {url.originalUrl}
                                            </a>
                                        </td>
                                        <td>
                                            <a
                                                href={url.shortUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="short-url-link"
                                                title="Click to visit"
                                            >
                                                {url.shortUrl}
                                            </a>
                                        </td>
                                        <td>
                                            <span className="badge bg-secondary">{url.createdBy}</span>
                                        </td>
                                        <td>
                                            <span className="text-muted">{url.createdDate}</span>
                                        </td>
                                        <td>
                                            <span className="badge bg-info text-dark">
                                                <i className="bi bi-eye"></i> {url.clickCount}
                                            </span>
                                        </td>
                                        <td>
                                            <div className="action-buttons">
                                                <a
                                                    href={`/ShortUrl/Info/${url.id}`}
                                                    className="btn btn-sm btn-info action-btn"
                                                    title="View details"
                                                >
                                                    <i className="bi bi-eye"></i> View
                                                </a>
                                                {canDelete(url) && (
                                                    <button
                                                        onClick={() => deleteUrl(url.id)}
                                                        className="btn btn-sm btn-danger action-btn"
                                                        disabled={deleting[url.id]}
                                                        title="Delete URL"
                                                    >
                                                        {deleting[url.id] ? (
                                                            <>
                                                                <span className="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                                                                Deleting...
                                                            </>
                                                        ) : (
                                                            <>
                                                                <i className="bi bi-trash"></i> Delete
                                                            </>
                                                        )}
                                                    </button>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default UrlShortenerApp;

