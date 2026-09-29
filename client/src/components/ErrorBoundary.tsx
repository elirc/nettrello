import { Component, type ErrorInfo, type ReactNode } from "react";

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false
  };

  public static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("Unexpected UI failure", error, errorInfo);
  }

  public render() {
    if (this.state.hasError) {
      return (
        <main className="empty-state">
          <h1>NetTrello</h1>
          <p>The board hit an unexpected rendering error. Refresh the page to recover.</p>
        </main>
      );
    }

    return this.props.children;
  }
}
