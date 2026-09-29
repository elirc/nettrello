import type { BoardDetail, BoardSummary, CardInput, CreateBoardInput, CreateColumnInput } from "../types";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5088";

interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
}

export class ApiError extends Error {
  public readonly status: number;
  public readonly code?: string;

  public constructor(message: string, status: number, code?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
  }
}

async function request<T>(path: string, options: RequestInit = {}, expectedBoardVersion?: number): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body) {
    headers.set("Content-Type", "application/json");
  }

  if (expectedBoardVersion !== undefined) {
    headers.set("X-Board-Version", String(expectedBoardVersion));
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers
  });

  if (!response.ok) {
    let problem: ProblemDetails | undefined;
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      problem = undefined;
    }

    throw new ApiError(problem?.detail ?? problem?.title ?? "Request failed.", response.status, problem?.code);
  }

  return (await response.json()) as T;
}

export const boardClient = {
  listBoards: () => request<BoardSummary[]>("/api/boards"),

  getBoard: (boardId: string) => request<BoardDetail>(`/api/boards/${boardId}`),

  createBoard: (input: CreateBoardInput) =>
    request<BoardDetail>("/api/boards", {
      method: "POST",
      body: JSON.stringify(input)
    }),

  addColumn: (boardId: string, input: CreateColumnInput, boardVersion: number) =>
    request<BoardDetail>(
      `/api/boards/${boardId}/columns`,
      {
        method: "POST",
        body: JSON.stringify(input)
      },
      boardVersion
    ),

  addCard: (boardId: string, columnId: string, input: CardInput, boardVersion: number) =>
    request<BoardDetail>(
      `/api/boards/${boardId}/columns/${columnId}/cards`,
      {
        method: "POST",
        body: JSON.stringify(input)
      },
      boardVersion
    ),

  updateCard: (boardId: string, cardId: string, input: CardInput, boardVersion: number) =>
    request<BoardDetail>(
      `/api/boards/${boardId}/cards/${cardId}`,
      {
        method: "PATCH",
        body: JSON.stringify(input)
      },
      boardVersion
    ),

  moveCard: (boardId: string, cardId: string, targetColumnId: string, position: number, boardVersion: number) =>
    request<BoardDetail>(
      `/api/boards/${boardId}/cards/${cardId}/move`,
      {
        method: "PUT",
        body: JSON.stringify({ targetColumnId, position })
      },
      boardVersion
    ),

  archiveCard: (boardId: string, cardId: string, boardVersion: number) =>
    request<BoardDetail>(
      `/api/boards/${boardId}/cards/${cardId}`,
      {
        method: "DELETE"
      },
      boardVersion
    )
};
