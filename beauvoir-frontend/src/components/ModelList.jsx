import React, { useEffect, useState } from 'react';
import axios from 'axios';

export default function ModelListWithSearch({ token }) {
  const [models, setModels] = useState([]);
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (query.trim() !== '') return;

    setLoading(true);
    setError(null);

    axios.get('http://localhost:5259/api/model', {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(response => {
        const sortedModels = response.data.sort(
          (a, b) => new Date(b.createdAt) - new Date(a.createdAt)
        );
        setModels(sortedModels);
      })
      .catch(() => setError('Error loading models.'))
      .finally(() => setLoading(false));
  }, [token, query]);

  const handleSearch = async (e) => {
    e.preventDefault();
    if (!query.trim()) {
      setModels([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await axios.get('http://localhost:5259/api/model/search', {
        headers: { Authorization: `Bearer ${token}` },
        params: { searchPart: query, page: 1, pageSize: 20 }
      });
      setModels(response.data.data || response.data);  // En tu ejemplo es array directo, por eso esta doble comprobación
    } catch (err) {
      setError('Error searching models.');
    } finally {
      setLoading(false);
    }
  };

   return (
    <div>
      <form onSubmit={handleSearch} style={{ marginBottom: '1rem' }}>
        <input
          type="text"
          value={query}
          onChange={e => setQuery(e.target.value)}
          placeholder="Search models by title, description, or tags"
          style={{ 
            width: '70%', 
            padding: '0.75rem',
            border: '1px solid #ddd',
            borderRadius: '4px'
          }}
        />
        <button 
          type="submit" 
          style={{ 
            padding: '0.75rem 1.5rem', 
            marginLeft: '0.5rem',
            background: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Search
        </button>
      </form>
      {loading && <p>Loading models...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {!loading && models.length === 0 && <p>No models available.</p>}
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', 
        gap: '1.5rem',
        marginTop: '1.5rem'
      }}>
        {models.map(model => (
          <div key={model.id} style={{ 
            border: '1px solid #e0e0e0', 
            borderRadius: '8px', 
            overflow: 'hidden',
            cursor: 'pointer',
            transition: 'transform 0.2s',
            ':hover': { 
              transform: 'translateY(-5px)',
              boxShadow: '0 4px 8px rgba(0,0,0,0.1)'
            }
          }}>
            <div style={{ 
              height: '180px', 
              backgroundColor: '#f5f5f5',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <span style={{ fontSize: '3rem' }}>📦</span>
            </div>
            <div style={{ padding: '1rem' }}>
              <h3 style={{ marginTop: 0 }}>{model.title}</h3>
              <p style={{ 
                color: '#666', 
                display: '-webkit-box',
                WebkitLineClamp: 2,
                WebkitBoxOrient: 'vertical',
                overflow: 'hidden'
              }}>
                {model.description || 'No description'}
              </p>
              <p style={{ margin: '0.5rem 0', fontSize: '0.9rem' }}>
                <strong>Author:</strong> {model.owner || 'Unknown'}
              </p>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                {model.tags?.slice(0, 3).map(tag => (
                  <span key={tag} style={{
                    padding: '0.25rem 0.5rem',
                    background: '#e9ecef',
                    borderRadius: '4px',
                    fontSize: '0.85rem'
                  }}>✔ {tag}</span>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
