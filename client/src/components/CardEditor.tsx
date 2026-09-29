import { FormEvent, useState } from "react";
import type { BoardMember, Card, CardInput, CardPriority } from "../types";

const priorities: CardPriority[] = ["Low", "Medium", "High", "Critical"];

interface CardEditorProps {
  card: Card;
  members: BoardMember[];
  onClose: () => void;
  onArchive: (cardId: string) => Promise<void>;
  onUpdate: (cardId: string, input: CardInput) => Promise<void>;
}

export function CardEditor({ card, members, onClose, onArchive, onUpdate }: CardEditorProps) {
  const [title, setTitle] = useState(card.title);
  const [description, setDescription] = useState(card.description);
  const [priority, setPriority] = useState<CardPriority>(card.priority);
  const [assigneeId, setAssigneeId] = useState(card.assigneeId ?? "");
  const [dueDate, setDueDate] = useState(card.dueDate?.slice(0, 10) ?? "");
  const [labels, setLabels] = useState(card.labels.join(", "));

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onUpdate(card.id, {
      title,
      description,
      priority,
      assigneeId: assigneeId || null,
      dueDate: dueDate ? new Date(`${dueDate}T12:00:00Z`).toISOString() : null,
      labels: labels
        .split(",")
        .map((label) => label.trim())
        .filter(Boolean)
    });
    onClose();
  }

  async function handleArchive() {
    await onArchive(card.id);
    onClose();
  }

  return (
    <div className="modal-backdrop" role="presentation" onMouseDown={onClose}>
      <section className="modal" role="dialog" aria-modal="true" aria-labelledby="card-editor-title" onMouseDown={(event) => event.stopPropagation()}>
        <form onSubmit={handleSubmit}>
          <div className="modal-header">
            <h2 id="card-editor-title">Edit card</h2>
            <button className="icon-button" type="button" aria-label="Close" onClick={onClose}>
              x
            </button>
          </div>

          <label>
            Title
            <input value={title} maxLength={120} onChange={(event) => setTitle(event.target.value)} />
          </label>

          <label>
            Description
            <textarea value={description} maxLength={2000} rows={5} onChange={(event) => setDescription(event.target.value)} />
          </label>

          <div className="form-grid">
            <label>
              Priority
              <select value={priority} onChange={(event) => setPriority(event.target.value as CardPriority)}>
                {priorities.map((item) => (
                  <option key={item} value={item}>
                    {item}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Assignee
              <select value={assigneeId} onChange={(event) => setAssigneeId(event.target.value)}>
                <option value="">Unassigned</option>
                {members.map((member) => (
                  <option key={member.id} value={member.id}>
                    {member.displayName}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="form-grid">
            <label>
              Due date
              <input type="date" value={dueDate} onChange={(event) => setDueDate(event.target.value)} />
            </label>

            <label>
              Labels
              <input value={labels} onChange={(event) => setLabels(event.target.value)} />
            </label>
          </div>

          <div className="modal-actions">
            <button type="submit">Save changes</button>
            <button className="danger-button" type="button" onClick={handleArchive}>
              Archive
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
