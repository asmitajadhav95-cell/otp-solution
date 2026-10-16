import { useEffect, useState } from "react";

// deadline = a timestamp in the browser's own clock (avoids server/client clock drift)
export function useCountdown(deadline: number | null): number {
  const [remaining, setRemaining] = useState(0);

  useEffect(() => {
    if (deadline === null) {
      setRemaining(0);
      return;
    }
    const tick = () => {
      const seconds = Math.max(0, Math.ceil((deadline - Date.now()) / 1000));
      setRemaining(seconds);
      if (seconds === 0) clearInterval(id);
    };
    const id = setInterval(tick, 250);
    tick();
    return () => clearInterval(id); // cleanup when deadline changes / component unmounts
  }, [deadline]);

  return remaining;
}