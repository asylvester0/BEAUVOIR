import React, { useEffect, useState } from 'react';

function ModelDashboard({ token }) {
  const API_BASE = 'http://localhost:5259/api/model'; // Ajusta si usas proxy
  const [models, setModels] = useState([]);
  const [selectedModel, setSelectedModel] = useState(null);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(false);
  const [file, setFile] = useState(null);
  const [tags, setTags] = useState([]);
  const [selectedTags, setSelectedTags] = useState([]);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(false);
  const [showUpload, setShowUpload] = useState(false);

  // ========================
  // Fetch modelos
  // ========================
  const fetchModels = async () => {
    setLoading(true);
    try {
      const res = await fetch(API_BASE, {
        headers: { Authorization: `Bearer ${token}` }
      });
      const data = await res.json();
      setModels(data.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)));
    } catch (err) {
      setError('Error charging models');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchModels();
  }, []);

  // ========================
  // Fetch tags
  // ========================
  useEffect(() => {
    fetch('http://localhost:5259/api/tag', {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(res => res.json())
      .then(setTags)
      .catch(() => setTags([]));
  }, []);

  // ========================
  // Subir modelo
  // ========================
  const handleUpload = async () => {
    if (!file) {
      setError('Select file');
      return;
    }

    const formData = new FormData();
    formData.append('title', title);
    formData.append('description', description);
    formData.append('isPublic', isPublic);
    selectedTags.forEach(id => formData.append('tagsId', id));
    formData.append('file', file);

    try {
      const res = await fetch(`${API_BASE}/upload`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
        body: formData
      });

      if (!res.ok) throw new Error(await res.text());

      alert('Models upload sucessfully');
      setTitle('');
      setDescription('');
      setIsPublic(false);
      setFile(null);
      setSelectedTags([]);
      fetchModels(); // actualiza lista
    } catch (err) {
      setError(err.message || 'Error uploading  model');
    }
  };

  // ========================
  // Descargar modelo
  // ========================
  const handleDownload = async (id, fileName) => {
    try {
      const res = await fetch(`${API_BASE}/download/${id}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) throw new Error('Error downloading');

      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.click();
      a.remove();
    } catch {
      alert('Model coudn´t download.');
    }
  };

  // ========================
  // Buscar modelos
  // ========================
  const handleSearch = async (e) => {
    e.preventDefault();
    if (!query.trim()) return fetchModels();

    setLoading(true);
    try {
      const response = await fetch(`${API_BASE}/search?searchPart=${query}&page=1&pageSize=20`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      const data = await response.json();
      setModels(data.data || data);
    } catch {
      setError('Error finding models');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2 style={{ margin: 0 }}>3D Models </h2>
        <button 
          onClick={() => setShowUpload(!showUpload)}
          style={{
            padding: '0.5rem 1rem',
            background: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          {showUpload ? 'Go back to list' : 'Upload new model'}
        </button>
      </div>

      {showUpload ?  (
        <div style={{ marginTop: '1rem' }}>
          <h3>Upload models</h3>
          {error && <p style={{ color: 'red' }}>{error}</p>}
          <input type="text" placeholder="Título" value={title} onChange={e => setTitle(e.target.value)} /><br />
          <textarea placeholder="Descripción" value={description} onChange={e => setDescription(e.target.value)} /><br />
          <label>
            ¿Public?
            <input type="checkbox" checked={isPublic} onChange={e => setIsPublic(e.target.checked)} />
          </label><br />
          <input type="file" onChange={e => setFile(e.target.files[0])} /><br />
          <label>Tags:</label>
          <select multiple value={selectedTags} onChange={e => {
            const selected = Array.from(e.target.selectedOptions, opt => parseInt(opt.value));
            setSelectedTags(selected);
          }}>
            {tags.map(tag => (
              <option key={tag.id} value={tag.id}>{tag.name}</option>
            ))}
          </select><br />
          <button onClick={handleUpload}>Upload model</button>
        </div>
      ) : (
        <>
          <form onSubmit={handleSearch} style={{ marginTop: '1rem' }}>
            <input
              type="text"
              value={query}
              onChange={e => setQuery(e.target.value)}
              placeholder="Find title or tags"
              style={{ width: '70%', padding: '0.5rem' }}
            />
            <button type="submit" style={{ padding: '0.5rem 1rem', marginLeft: '0.5rem' }}>
              Search
            </button>
          </form>

          {loading && <p>Charging models...</p>}
          {error && <p style={{ color: 'red' }}>{error}</p>}
          {!loading && models.length === 0 && <p>No models avaliable.</p>}

          <div style={{ 
            display: 'grid', 
            gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', 
            gap: '1.5rem',
            marginTop: '1.5rem'
          }}>
            {models.map(model => (
              <div key={model.id} 
                   onClick={() => setSelectedModel(model)}
                   style={{ 
                     border: '1px solid #e0e0e0', 
                     borderRadius: '8px', 
                     overflow: 'hidden',
                     cursor: 'pointer',
                     transition: 'transform 0.2s',
                     ':hover': { transform: 'translateY(-5px)' }
                   }}>
                <div style={{ 
                  height: '180px', 
                  backgroundColor: '#f5f5f5',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center'
                }}>
                  {/* Placeholder for model preview */}
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


          {selectedModel && (
            <div style={{ 
              position: 'fixed', 
              top: 0, 
              left: 0, 
              right: 0, 
              bottom: 0, 
              backgroundColor: 'rgba(0,0,0,0.5)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              zIndex: 1000
            }}>
              <div style={{ 
                backgroundColor: 'white', 
                width: '80%', 
                maxWidth: '800px',
                borderRadius: '8px',
                padding: '2rem'
              }}>
                <h1 style={{ marginTop: 0 }}>{selectedModel.title}</h1>
                <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
                  {selectedModel.tags?.map(tag => (
                    <span key={tag} style={{
                      padding: '0.25rem 0.5rem',
                      background: '#e9ecef',
                      borderRadius: '4px',
                    }}>✔ {tag}</span>
                  ))}
                </div>
                <p>{selectedModel.description}</p>
                <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem' }}>
                  <button 
                    onClick={() => handleDownload(selectedModel.id, selectedModel.fileName)}
                    style={{
                      padding: '0.75rem 1.5rem',
                      background: '#007bff',
                      color: 'white',
                      border: 'none',
                      borderRadius: '4px',
                      cursor: 'pointer'
                    }}
                  >
                    Download
                  </button>
                 <button 
                  onClick={() => setSelectedModel(null)}
                  style={{
                    padding: '0.75rem 1.5rem',
                    background: '#6c757d',
                    color: 'white',
                    border: 'none',
                    borderRadius: '4px',
                    cursor: 'pointer'
                  }}
                >
                  Close
                </button>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
export default ModelDashboard;
