import { useCallback, useEffect, useMemo, useState } from "react";
import { ApiError, boardClient } from "../api/boardClient";
import type { BoardDetail, BoardSummary, CardInput, CreateBoardInput, CreateColumnInput } from "../types";

export function useBoard() {
  const [boards, setBoards] = useState<BoardSummary[]>([]);
  const [selectedBoardId, setSelectedBoardId] = useState<string | null>(null);
  const [board, setBoard] = useState<BoardDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refreshBoards = useCallback(async () => {
    const summaries = await boardClient.listBoards();
    setBoards(summaries);
    setSelectedBoardId((current) => current ?? summaries[0]?.id ?? null);
  }, []);

  useEffect(() => {
    let isCurrent = true;

    async function loadInitialData() {
      setIsLoading(true);
      setError(null);
      try {
        const summaries = await boardClient.listBoards();
        if (!isCurrent) {
          return;
        }

        setBoards(summaries);
        setSelectedBoardId(summaries[0]?.id ?? null);
      } catch (loadError) {
        if (isCurrent) {
          setError(toMessage(loadError));
        }
      } finally {
        if (isCurrent) {
          setIsLoading(false);
        }
      }
    }

    void loadInitialData();

    return () => {
      isCurrent = false;
    };
  }, []);

  useEffect(() => {
    if (!selectedBoardId) {
      setBoard(null);
      return;
    }

    let isCurrent = true;

    async function loadBoard() {
      setIsLoading(true);
      setError(null);
      try {
        const detail = await boardClient.getBoard(selectedBoardId);
        if (isCurrent) {
          setBoard(detail);
        }
      } catch (loadError) {
        if (isCurrent) {
          setError(toMessage(loadError));
        }
      } finally {
        if (isCurrent) {
          setIsLoading(false);
        }
      }
    }

    void loadBoard();

    return () => {
      isCurrent = false;
    };
  }, [selectedBoardId]);

  const mutateBoard = useCallback(
    async (mutation: (current: BoardDetail) => Promise<BoardDetail>) => {
      if (!board) {
        return;
      }

      setIsSaving(true);
      setError(null);

      try {
        const updated = await mutation(board);
        setBoard(updated);
        await refreshBoards();
      } catch (saveError) {
        setError(toMessage(saveError));
        if (saveError instanceof ApiError && saveError.status === 409) {
          const refreshed = await boardClient.getBoard(board.id);
          setBoard(refreshed);
        }
      } finally {
        setIsSaving(false);
      }
    },
    [board, refreshBoards]
  );

  const actions = useMemo(
    () => ({
      selectBoard: setSelectedBoardId,

      createBoard: async (input: CreateBoardInput) => {
        setIsSaving(true);
        setError(null);
        try {
          const created = await boardClient.createBoard(input);
          setBoard(created);
          setSelectedBoardId(created.id);
          await refreshBoards();
        } catch (createError) {
          setError(toMessage(createError));
        } finally {
          setIsSaving(false);
        }
      },

      addColumn: (input: CreateColumnInput) =>
        mutateBoard((current) => boardClient.addColumn(current.id, input, current.version)),

      addCard: (columnId: string, input: CardInput) =>
        mutateBoard((current) => boardClient.addCard(current.id, columnId, input, current.version)),

      updateCard: (cardId: string, input: CardInput) =>
        mutateBoard((current) => boardClient.updateCard(current.id, cardId, input, current.version)),

      moveCard: (cardId: string, targetColumnId: string, position: number) =>
        mutateBoard((current) => boardClient.moveCard(current.id, cardId, targetColumnId, position, current.version)),

      archiveCard: (cardId: string) =>
        mutateBoard((current) => boardClient.archiveCard(current.id, cardId, current.version)),

      dismissError: () => setError(null)
    }),
    [mutateBoard, refreshBoards]
  );

  return {
    boards,
    selectedBoardId,
    board,
    isLoading,
    isSaving,
    error,
    actions
  };
}

function toMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return "Something went wrong.";
}
