import React, { useState } from "react";
import axios from "axios";

function App() {
  const [status, setStatus] = useState("");

  const checkBackend = async () => {
    try {
      const res = await axios.get("https://localhost:7020/api/health");
      setStatus(JSON.stringify(res.data));
    } catch (err) {
      setStatus("Error: " + err.message);
    }
  };

  return (
    <div style={{ padding: "20px" }}>
      <h1>Learnlytics</h1>
      <button onClick={checkBackend}>Check Backend Status</button>
      <p>{status}</p>
    </div>
  );
}

export default App;
