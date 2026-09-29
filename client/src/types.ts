export type CardPriority = "Low" | "Medium" | "High" | "Critical";

export interface BoardSummary {
  id: string;
  name: string;
  description: string;
  cardCount: number;
  updatedAt: string;
}

export interface BoardDetail {
  id: string;
  name: string;
  description: string;
  columns: BoardColumn[];
  members: BoardMember[];
  version: number;
  updatedAt: string;
}

export interface BoardColumn {
  id: string;
  title: string;
  position: number;
  cardLimit: number;
  cards: Card[];
}

export interface Card {
  id: string;
  title: string;
  description: string;
  priority: CardPriority;
  assigneeId: string | null;
  assigneeName: string | null;
  dueDate: string | null;
  labels: string[];
  position: number;
  updatedAt: string;
}

export interface BoardMember {
  id: string;
  displayName: string;
  email: string;
}

export interface CreateBoardInput {
  name: string;
  description?: string;
}

export interface CreateColumnInput {
  title: string;
  cardLimit?: number;
}

export interface CardInput {
  title: string;
  description?: string;
  priority: CardPriority;
  assigneeId?: string | null;
  dueDate?: string | null;
  labels?: string[];
}
