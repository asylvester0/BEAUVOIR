import React, { useState, useEffect } from 'react';

function ModelPage() {
  const [models, setModels] = useState([]);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(false);
  const [file, setFile] = useState(null);
  const [tags, setTags] = useState([]);       // lista de tags desde backend
  const [selectedTags, setSelectedTags] = useState([]); // IDs seleccionados
  const [error, setError] = useState('');

  const API_BASE = '/api/model'; // cambiar si usas proxy

  const token = localStorage.getItem('token');

  // ========================
  // Cargar modelos visibles
  // ========================
  useEffect(() => {
    fetch(API_BASE, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })
      .then(res => res.json())
      .then(data => setModels(data))
      .catch(err => setError("Error fetching models"));
  }, []);

  // ========================
  // Cargar tags (si se usan)
  // ========================
  useEffect(() => {
    fetch('/api/tag', { // ajusta si tu endpoint es diferente
      headers: { 'Authorization': `Bearer ${token}` }
    })
      .then(res => res.json())
      .then(data => setTags(data))
      .catch(() => setTags([]));
  }, []);

  // ========================
  // Subir modelo
  // ========================
  const handleUpload = async () => {
    if (!file) {
      setError('Select file (.obj or .fbx)');
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
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      if (!res.ok) {
        const msg = await res.text();
        throw new Error(msg);
      }

      alert('Model correctly upload');
      setTitle('');
      setDescription('');
      setIsPublic(false);
      setFile(null);
      setSelectedTags([]);
      const newModel = await res.json();
      setModels(prev => [...prev, newModel]);
    } catch (err) {
      setError(err.message || 'Error uploading model');
    }
  };

  // ========================
  // Descargar modelo
  // ========================
  const handleDownload = async (id, fileName) => {
    try {
      const res = await fetch(`${API_BASE}/download/${id}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      if (!res.ok) throw new Error('Error downloading');

      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.click();
      a.remove();
    } catch (err) {
      alert('Model could not download.');
    }
  };

  return (
    <div style={{ padding: '2rem' }}>
      <h2>Subir nuevo modelo</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}

      <input
        type="text"
        placeholder="Title"
        value={title}
        onChange={e => setTitle(e.target.value)}
      /><br />

      <textarea
        placeholder="Description"
        value={description}
        onChange={e => setDescription(e.target.value)}
      /><br />

      <label>
        ¿Public?
        <input
          type="checkbox"
          checked={isPublic}
          onChange={e => setIsPublic(e.target.checked)}
        />
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

      <button onClick={handleUpload}>Upload</button>

      <hr />

      <h2>Models avaliables</h2>
      <ul>
        {models.map(model => (
          <li key={model.id}>
            <strong>{model.name}</strong> - {model.description}
            <button onClick={() => handleDownload(model.id, model.fileName)}>Download</button>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default ModelPage;
