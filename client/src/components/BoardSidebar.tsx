import { FormEvent, useState } from "react";
import type { BoardSummary } from "../types";

interface BoardSidebarProps {
  boards: BoardSummary[];
  selectedBoardId: string | null;
  onSelectBoard: (boardId: string) => void;
  onCreateBoard: (input: { name: string; description?: string }) => Promise<void>;
}

export function BoardSidebar({ boards, selectedBoardId, onSelectBoard, onCreateBoard }: BoardSidebarProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!name.trim()) {
      return;
    }

    await onCreateBoard({ name, description });
    setName("");
    setDescription("");
  }

  return (
    <aside className="sidebar" aria-label="Boards">
      <div className="sidebar-section">
        <h2>Boards</h2>
        <div className="board-list">
          {boards.map((board) => (
            <button
              className={board.id === selectedBoardId ? "board-list-item active" : "board-list-item"}
              key={board.id}
              type="button"
              onClick={() => onSelectBoard(board.id)}
            >
              <span>{board.name}</span>
              <small>{board.cardCount} cards</small>
            </button>
          ))}
        </div>
      </div>

      <form className="sidebar-section board-form" onSubmit={handleSubmit}>
        <h2>New board</h2>
        <label>
          Name
          <input value={name} maxLength={100} onChange={(event) => setName(event.target.value)} />
        </label>
        <label>
          Description
          <textarea value={description} maxLength={500} rows={3} onChange={(event) => setDescription(event.target.value)} />
        </label>
        <button type="submit">Create board</button>
      </form>
    </aside>
  );
}
