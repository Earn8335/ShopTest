"use client";

import { useEffect, useState } from "react";
import * as api from "@/app/lib/api";
import styles from "./page.module.css";

const money = (value: number) =>
  value.toLocaleString("th-TH", { minimumFractionDigits: 2 });

export default function Home() {
  const [products, setProducts] = useState<api.Product[]>([]);
  const [cart, setCart] = useState<api.Cart | null>(null);
  const [order, setOrder] = useState<api.Order | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function run(action: () => Promise<unknown>) {
    setBusy(true);
    setError("");
    try {
      await action();
    } catch (e) {
      setError(e instanceof Error ? e.message : "เกิดข้อผิดพลาด");
    }
    try {
      setProducts(await api.getProducts());
      setCart(await api.getCart());
    } catch {
      setError(
        "เชื่อมต่อ API ไม่ได้ ตรวจว่า Backend รันอยู่ที่ localhost:5000",
      );
    }
    setBusy(false);
  }

  useEffect(() => {
    run(async () => {});
  }, []);

  return (
    <main className={styles.page}>
      <h1>Shopping IT</h1>

      <h2>สินค้า</h2>
      <table className={styles.table}>
        <thead>
          <tr>
            <th>รหัส</th>
            <th>ชื่อสินค้า</th>
            <th>ราคา</th>
            <th>คงเหลือ</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.productId}>
              <td>{p.productCode}</td>
              <td>{p.productName}</td>
              <td>{money(p.unitPrice)}</td>
              <td>{p.stockQuantity}</td>
              <td>
                <button
                  disabled={busy}
                  onClick={() => {
                    if (p.stockQuantity === 0) {
                      alert("สินค้าหมดสต็อก ไม่สามารถเพิ่มลงตะกร้าได้");
                      return;
                    }
                    setOrder(null);
                    run(() => api.addToCart(p.productId));
                  }}
                >
                  {p.stockQuantity === 0 ? "หมด" : "เพิ่มลงตะกร้า"}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>ตะกร้า</h2>
      {!cart || cart.items.length === 0 ? (
        <p>ตะกร้าว่าง</p>
      ) : (
        <>
          <table className={styles.table}>
            <thead>
              <tr>
                <th>ชื่อสินค้า</th>
                <th>ราคา</th>
                <th>จำนวน</th>
                <th>รวม</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {cart.items.map((i) => (
                <tr key={i.productId}>
                  <td>{i.productName}</td>
                  <td>{money(i.unitPrice)}</td>
                  <td>
                    <button
                      disabled={busy}
                      onClick={() =>
                        run(() => api.setQuantity(i.productId, i.quantity - 1))
                      }
                    >
                      −
                    </button>
                    <span className={styles.quantity}>{i.quantity}</span>
                    <button
                      disabled={busy}
                      onClick={() =>
                        run(() => api.setQuantity(i.productId, i.quantity + 1))
                      }
                    >
                      +
                    </button>
                  </td>
                  <td>{money(i.lineTotal)}</td>
                  <td>
                    <button
                      disabled={busy}
                      onClick={() => run(() => api.removeItem(i.productId))}
                    >
                      ลบ
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          
          {error && (
            <p className={styles.error} style={{ color: "#d01716" }}>
              {error}
            </p>
          )}

          <p className={styles.total}>ยอดรวม {money(cart.totalAmount)} บาท</p>
          <div className={styles.actions}>
            <button disabled={busy} onClick={() => run(() => api.clearCart())}>
              ล้างตะกร้า
            </button>
            <button
              disabled={busy}
              onClick={() => run(async () => setOrder(await api.checkout()))}
            >
              Checkout
            </button>
          </div>
        </>
      )}

      {order && (
        <div className={styles.order}>
          <h2>สั่งซื้อสำเร็จ — Order #{order.orderId}</h2>
          <ul>
            {order.items.map((i) => (
              <li key={i.productId}>
                {i.productName} × {i.quantity} = {money(i.lineTotal)}
              </li>
            ))}
          </ul>
          <p className={styles.total}>
            ยอดที่ต้องชำระ {money(order.totalAmount)} บาท
          </p>
        </div>
      )}
    </main>
  );
}
