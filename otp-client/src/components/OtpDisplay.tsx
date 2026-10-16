type Props = { code: string; remaining: number };

export function OtpDisplay({ code, remaining }: Props) {
  const expired = remaining === 0;
  return (
    <section className="card">
      <p className={`otp ${expired ? "expired" : ""}`}>{code}</p>
      <p>{expired ? "Expired — generate a new password." : `Valid for ${remaining}s`}</p>
      <progress max={30} value={remaining} />
    </section>
  );
}