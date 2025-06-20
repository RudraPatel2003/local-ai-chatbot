import { JSX, useEffect, useState } from "react";

import MarkdownMessage from "./markdown-message";

const MESSAGE_UPDATE_SPEED_IN_MS = 3;

type TypingMessageProps = {
  content: string;
};

export function TypingMessage({ content }: TypingMessageProps): JSX.Element {
  const [displayedContent, setDisplayedContent] = useState("");
  const [currentIndex, setCurrentIndex] = useState(0);

  useEffect((): (() => void) | undefined => {
    if (currentIndex < content.length) {
      const timeout = setTimeout(() => {
        setDisplayedContent((previous) => previous + content[currentIndex]);
        setCurrentIndex((previous) => previous + 1);
      }, MESSAGE_UPDATE_SPEED_IN_MS);

      return () => clearTimeout(timeout);
    }
    return undefined;
  }, [content, currentIndex]);

  return (
    <div className="relative">
      <MarkdownMessage content={displayedContent} />
    </div>
  );
}
