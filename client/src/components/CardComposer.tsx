import { FormEvent, useState } from "react";
import type { BoardMember, CardInput, CardPriority } from "../types";

const priorities: CardPriority[] = ["Low", "Medium", "High", "Critical"];

interface CardComposerProps {
  members: BoardMember[];
  onCreateCard: (input: CardInput) => Promise<void>;
}

export function CardComposer({ members, onCreateCard }: CardComposerProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [title, setTitle] = useState("");
  const [priority, setPriority] = useState<CardPriority>("Medium");
  const [assigneeId, setAssigneeId] = useState("");
  const [labels, setLabels] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!title.trim()) {
      return;
    }

    await onCreateCard({
      title,
      priority,
      assigneeId: assigneeId || null,
      labels: labels
        .split(",")
        .map((label) => label.trim())
        .filter(Boolean)
    });
    setTitle("");
    setLabels("");
    setIsOpen(false);
  }

  if (!isOpen) {
    return (
      <button className="ghost-button full-width" type="button" onClick={() => setIsOpen(true)}>
        + Add card
      </button>
    );
  }

  return (
    <form className="card-composer" onSubmit={handleSubmit}>
      <input autoFocus placeholder="Card title" value={title} maxLength={120} onChange={(event) => setTitle(event.target.value)} />
      <div className="inline-fields">
        <select value={priority} onChange={(event) => setPriority(event.target.value as CardPriority)}>
          {priorities.map((item) => (
            <option key={item} value={item}>
              {item}
            </option>
          ))}
        </select>
        <select value={assigneeId} onChange={(event) => setAssigneeId(event.target.value)}>
          <option value="">Unassigned</option>
          {members.map((member) => (
            <option key={member.id} value={member.id}>
              {member.displayName}
            </option>
          ))}
        </select>
      </div>
      <input placeholder="Labels, comma separated" value={labels} onChange={(event) => setLabels(event.target.value)} />
      <div className="button-row">
        <button type="submit">Add</button>
        <button className="ghost-button" type="button" onClick={() => setIsOpen(false)}>
          Cancel
        </button>
      </div>
    </form>
  );
}
