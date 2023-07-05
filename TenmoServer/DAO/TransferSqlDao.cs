﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography.Xml;
using TenmoServer.Exceptions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDao : ITransferDao
    {
        private readonly string ConnectionString;

        public TransferSqlDao(string dbconnectionString)
        {
            ConnectionString = dbconnectionString;
        }

        public Transfer GetTransferByTransferId(int transferId)
        {
            Transfer transfer = null;
            string sql = "SELECT * FROM transfer " +
                "WHERE transfer_id = @transfer_id";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@transfer_id", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                       transfer = MapRowToTransfer(reader);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return transfer;
        }

        public List<Transfer> GetTransfersForUser(int accountId)
        {
            List<Transfer> transfers = null;
            string sql = "SELECT * FROM transfer " +
                "WHERE account_from = @account_from OR account_to = @account_to";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    cmd.Parameters.AddWithValue("@account_to", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        transfers.Add(MapRowToTransfer(reader));
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return transfers;
        }

        public Transfer UpdateTransfer(Transfer transfer)
        {
            Transfer updatedTransfer = null;
            string sql = "UPDATE transfer SET transfer_type_id = @transfer_type_id, " +
                "transfer_status_id = @transfer_status_id, " +
                "account_from = @account_from, " +
                "account_to = @account_to, " +
                "amount = @amount " +
                "WHERE transfer_id = @transfer_id";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", transfer.TransferTypeId);
                    cmd.Parameters.AddWithValue("@transfer_status_id", transfer.TransferStatusId);
                    cmd.Parameters.AddWithValue("@account_from", transfer.AccountFrom);
                    cmd.Parameters.AddWithValue("@account_to", transfer.AccountTo);
                    cmd.Parameters.AddWithValue("@amount", transfer.Amount);
                    int numberOfRows = cmd.ExecuteNonQuery();
                    if(numberOfRows == 0)
                    {
                        throw new DaoException("Zero rows affected, expected at least one");
                    }
                   
               }
                updatedTransfer = GetTransferByTransferId(transfer.TransferId);
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return updatedTransfer;

        }

        public Transfer CreateTransfer(Transfer newTransfer)
        {
            Transfer addedTransfer = null;
            string sql = "INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount) " +
                "OUTPUT INSERTED.transfer_id " +
                "VALUES (@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount";

            try
            {
                int newTransferId;
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", newTransfer.TransferTypeId);
                    cmd.Parameters.AddWithValue("@transfer_status_id", newTransfer.TransferStatusId);
                    cmd.Parameters.AddWithValue("@account_from", newTransfer.AccountFrom);
                    cmd.Parameters.AddWithValue("@account_to", newTransfer.AccountTo);
                    cmd.Parameters.AddWithValue("@amount", newTransfer.Amount);

                    newTransferId = Convert.ToInt32(cmd.ExecuteScalar());

                }

                addedTransfer = GetTransferByTransferId(newTransferId);
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return addedTransfer;
        }

        public string GetFromUserById(int id)
        {
            string user = null;
            string sql = "SELECT * FROM transfer as t " +
                "JOIN account as ON t.account_from = a.account_id " +
                "JOIN tenmo_user as tu ON a.user_id = tu.user_id " +
                "WHERE t.account_from = @account_from";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@accout_from", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user = Convert.ToString(reader["username"]);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return user;
        }

        public string GetToUserById(int id)
        {
            string user = null;
            string sql = "SELECT * FROM transfer as t " +
                "JOIN account as ON t.account_from = a.account_id " +
                "JOIN tenmo_user as tu ON a.user_id = tu.user_id " +
                "WHERE t.account_to = @account_to";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@account_to", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user = Convert.ToString(reader["username"]);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return user;
        }





        public Transfer MapRowToTransfer(SqlDataReader reader)
        {
            Transfer transfer = new Transfer();
            transfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
            transfer.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            transfer.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            transfer.AccountFrom = Convert.ToInt32(reader["account_from"]);
            transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
            transfer.Amount = Convert.ToDecimal(reader["amount"]);

            return transfer;
            
        }


    }
}
